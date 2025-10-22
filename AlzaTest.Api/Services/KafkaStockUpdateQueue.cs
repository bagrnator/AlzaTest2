using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlzaTest.Api.Services
{
    public class KafkaStockUpdateQueue : IStockUpdateQueue
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaStockUpdateQueue(IConfiguration configuration)
        { 
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _topic = configuration["Kafka:StockUpdateTopic"];
        }

        public async Task EnqueueAsync(StockUpdate stockUpdate)
        {
            var message = JsonSerializer.Serialize(stockUpdate);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
        }

        public Task<StockUpdate> DequeueAsync()
        {
            // This queue is producer-only. The consumer is in StockUpdateService.
            throw new System.NotImplementedException();
        }
    }
}
