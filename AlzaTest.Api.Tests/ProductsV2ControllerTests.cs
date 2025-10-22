using AlzaTest.Data;
using AlzaTest.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlzaTest.Api.Controllers.V1;
using AlzaTest.Api.Controllers.V2;
using AlzaTest.Api.Services;
using AlzaTest.Data.Data;
using ProductsController = AlzaTest.Api.Controllers.V1.ProductsController;
using V2 = AlzaTest.Api.Controllers.V2;

namespace AlzaTest.Api.Tests
{
    public class ProductsControllerTests
    {
        private DbContextOptions<ProductDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            // Arrange
            DbContextOptions<ProductDbContext> options = CreateNewContextOptions();
            await using (ProductDbContext context = new(options))
            {
                context.Products.AddRange(
                    new Product { Id = 1, Name = "Product 1", ImageUrl = "url1" },
                    new Product { Id = 2, Name = "Product 2", ImageUrl = "url2" }
                );
                await context.SaveChangesAsync();
            }

            await using (ProductDbContext context = new(options))
            {
                V2.ProductsController controller = new(context);

                // Act
                ActionResult<IEnumerable<Product>> result = await controller.GetProducts();

                // Assert
                ActionResult<IEnumerable<Product>> actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
                IEnumerable<Product> model = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenProductExists()
        {
            // Arrange
            DbContextOptions<ProductDbContext> options = CreateNewContextOptions();
            await using (ProductDbContext context = new(options))
            {
                context.Products.Add(new Product { Id = 1, Name = "Product 1", ImageUrl = "url1" });
                await context.SaveChangesAsync();
            }

            await using (ProductDbContext context = new(options))
            {
                ProductsController controller = new ProductsController(context, new InMemoryStockUpdateQueue());

                // Act
                ActionResult<Product> result = await controller.GetProduct(1);

                // Assert
                ActionResult<Product> actionResult = Assert.IsType<ActionResult<Product>>(result);
                Product model = Assert.IsType<Product>(actionResult.Value);
                Assert.Equal(1, model.Id);
            }
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            DbContextOptions<ProductDbContext> options = CreateNewContextOptions();
            await using ProductDbContext context = new(options);
            ProductsController controller = new(context, new InMemoryStockUpdateQueue());

            // Act
            ActionResult<Product> result = await controller.GetProduct(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostProduct_CreatesProduct()
        {
            // Arrange
            DbContextOptions<ProductDbContext> options = CreateNewContextOptions();
            Product newProduct = new() { Name = "New Product", ImageUrl = "new_url" };

            await using ProductDbContext context = new(options);
            ProductsController controller = new(context, new InMemoryStockUpdateQueue());

            // Act
            ActionResult<Product> result = await controller.PostProduct(newProduct);

            // Assert
            CreatedAtActionResult actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Product model = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal("New Product", model.Name);
        }

        [Fact]
        public async Task PatchProductStock_UpdatesStock()
        {
            // Arrange
            DbContextOptions<ProductDbContext> options = CreateNewContextOptions();
            await using (ProductDbContext context = new(options))
            {
                context.Products.Add(new Product { Id = 1, Name = "Product 1", ImageUrl = "url1", Quantity = 10 });
                await context.SaveChangesAsync();
            }

            await using (ProductDbContext context = new(options))
            {
                ProductsController controller = new(context, new InMemoryStockUpdateQueue());

                // Act
                IActionResult result = await controller.PatchProductStock(1, 20);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Product? product = await context.Products.FindAsync(1);
                Assert.NotNull(product);
                Assert.Equal(20, product.Quantity);
            }
        }
    }
}
