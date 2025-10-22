using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlzaTest.Api.Services
{
    public class KafkaStockUpdateQueue : IStockUpdateQueue
    {
        private readonly KafkaProducerFactory _producerFactory;
        private readonly string _topic;
        private readonly ILogger<KafkaStockUpdateQueue> _logger;

        public KafkaStockUpdateQueue(IConfiguration configuration, KafkaProducerFactory producerFactory, ILogger<KafkaStockUpdateQueue> logger)
        {
            _topic = configuration["Kafka:StockUpdateTopic"];
            _producerFactory = producerFactory;
            _logger = logger;
        }

        public Task EnqueueAsync(StockUpdate stockUpdate)
        {
            var producer = _producerFactory.GetProducer();
            var message = JsonSerializer.Serialize(stockUpdate);

            // Use the non-blocking Produce method with a delivery handler callback
            producer.Produce(_topic, new Message<Null, string> { Value = message }, (deliveryReport) =>
            {
                if (deliveryReport.Error.Code != ErrorCode.NoError)
                {
                    _logger.LogError($"Failed to deliver message: {deliveryReport.Error.Reason}");
                }
                else
                {
                    _logger.LogInformation($"Message for product {stockUpdate.ProductId} delivered to {deliveryReport.TopicPartitionOffset}");
                }
            });

            return Task.CompletedTask;
        }

        public Task<StockUpdate> DequeueAsync()
        {
            // This queue is producer-only. The consumer is in StockUpdateService.
            throw new System.NotImplementedException();
        }
    }
}