using AlzaTest.Api.Services;
using AlzaTest.Data.Data;
using AlzaTest.Data.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlzaTest.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductsController(ProductDbContext context, IStockUpdateQueue stockUpdateQueue) : ControllerBase
    {
        // GET: api/v1/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await context.Products.ToListAsync();
        }

        // GET: api/v1/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            Product? product = await context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/v1/products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            context.Products.Add(product);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PATCH: api/v1/products/5/stock
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> PatchProductStock(int id, [FromBody] int quantity)
        {
            var product = await context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            stockUpdateQueue.Enqueue(new StockUpdate(id, quantity));

            return Accepted();
        }
    }
}
