using AlzaTest.Data.Data;
using Confluent.Kafka;
using System.Text.Json;
using AlzaTest.Data.Entities;

namespace AlzaTest.Api.Services
{
    public class StockUpdateService : BackgroundService
    {
        private readonly ILogger<StockUpdateService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public StockUpdateService(ILogger<StockUpdateService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                ConsumerConfig consumerConfig = new()
                {
                    BootstrapServers = _configuration["Kafka:BootstrapServers"],
                    GroupId = "stock-update-consumer-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                string? topic = _configuration["Kafka:StockUpdateTopic"];

                using IConsumer<Ignore, string>? consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
                consumer.Subscribe(topic);
                _logger.LogInformation($"Subscribed to Kafka topic: {topic}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (await ConsumeAsync(stoppingToken, consumer))
                        continue;
                    break;
                }

                consumer.Close();
            }, stoppingToken);
        }

        private async Task<bool> ConsumeAsync(CancellationToken stoppingToken, IConsumer<Ignore, string> consumer)
        {
            try
            {
                ConsumeResult<Ignore, string>? consumeResult = consumer.Consume(stoppingToken);
                StockUpdate? stockUpdate = JsonSerializer.Deserialize<StockUpdate>(consumeResult.Message.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (stockUpdate == null) return true;
                using IServiceScope scope = _scopeFactory.CreateScope();
                ProductDbContext dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                Product? product = await dbContext.Products.FindAsync(new object[] { stockUpdate.ProductId }, cancellationToken: stoppingToken);

                if (product != null)
                {
                    product.Quantity = stockUpdate.Quantity;
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"Stock for product {product.Id} updated to {product.Quantity}");
                }
                else
                {
                    _logger.LogWarning($"Product with ID {stockUpdate.ProductId} not found.");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message.");
            }

            return true;
        }
    }
}
