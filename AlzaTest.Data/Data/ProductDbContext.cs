using AlzaTest.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlzaTest.Data.Data
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        public void Seed()
        {
            var productsToSeed = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "A high-end laptop for all your needs.",
                    Price = 1200.50m,
                    Quantity = 15,
                    ImageUrl = "https://via.placeholder.com/150/92c952"
                },
                new Product
                {
                    Id = 2,
                    Name = "Mouse",
                    Description = "Ergonomic wireless mouse.",
                    Price = 25.00m,
                    Quantity = 100,
                    ImageUrl = "https://via.placeholder.com/150/771796"
                },
                new Product
                {
                    Id = 3,
                    Name = "Keyboard",
                    Description = "Mechanical keyboard with RGB lighting.",
                    Price = 75.99m,
                    Quantity = 50,
                    ImageUrl = "https://via.placeholder.com/150/24f355"
                }
            };

            var existingProductIds = Products.Select(p => p.Id).ToList();
            var productsToAdd = productsToSeed.Where(p => !existingProductIds.Contains(p.Id)).ToList();

            if (productsToAdd.Any())
            {
                Products.AddRange(productsToAdd);
                SaveChanges();
            }
        }
    }
}
