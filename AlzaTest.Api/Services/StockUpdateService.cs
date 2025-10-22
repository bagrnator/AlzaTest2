using AlzaTest.Data.Data;
using Confluent.Kafka;
using System.Text.Json;
using AlzaTest.Data.Entities;

namespace AlzaTest.Api.Services
{
    public class StockUpdateService(ILogger<StockUpdateService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConsumerConfig consumerConfig = new()
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "stock-update-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            string? topic = configuration["Kafka:StockUpdateTopic"];

            using IConsumer<Ignore, string>? consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe(topic);
            logger.LogInformation($"Subscribed to Kafka topic: {topic}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<Ignore, string>? consumeResult = consumer.Consume(stoppingToken);
                    StockUpdate? stockUpdate = JsonSerializer.Deserialize<StockUpdate>(consumeResult.Message.Value);

                    if (stockUpdate == null) continue;
                    using IServiceScope scope = scopeFactory.CreateScope();
                    ProductDbContext dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                    Product? product = await dbContext.Products.FindAsync(new object[] { stockUpdate.ProductId }, cancellationToken: stoppingToken);

                    if (product != null)
                    {
                        product.Quantity = stockUpdate.Quantity;
                        await dbContext.SaveChangesAsync(stoppingToken);
                        logger.LogInformation($"Stock for product {product.Id} updated to {product.Quantity}");
                    }
                    else
                    {
                        logger.LogWarning($"Product with ID {stockUpdate.ProductId} not found.");
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Kafka consumer stopping.");
                    break;
                }
                catch (Exception ex)
                { 
                    logger.LogError(ex, "Error processing Kafka message.");
                }
            }

            consumer.Close();
        }
    }
}
