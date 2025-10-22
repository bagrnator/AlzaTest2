using AlzaTest.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using AlzaTest.Data.Data;

namespace AlzaTest.Api.Services
{
    public class StockUpdateService : BackgroundService
    {
        private readonly ILogger<StockUpdateService> _logger;
        private readonly IStockUpdateQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public StockUpdateService(ILogger<StockUpdateService> logger, IStockUpdateQueue queue, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stockUpdate = await _queue.DequeueAsync();

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                    var product = await dbContext.Products.FindAsync(stockUpdate.ProductId);

                    if (product != null)
                    {
                        product.Quantity = stockUpdate.Quantity;
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"Stock for product {product.Id} updated to {product.Quantity}");
                    }
                }
            }
        }
    }
}
