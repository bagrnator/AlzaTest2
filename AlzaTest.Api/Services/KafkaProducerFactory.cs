using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace AlzaTest.Api.Services
{
    public class KafkaProducerFactory
    {
        private readonly ProducerConfig _producerConfig;
        private IProducer<Null, string> _producer;
        private readonly Lock _lock = new();

        public KafkaProducerFactory(IConfiguration configuration)
        {
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };
        }

        public IProducer<Null, string> GetProducer()
        {
            if (_producer != null) 
                return _producer;
            
            lock (_lock)
            {
                // Double-check locking to ensure thread safety
                if (_producer == null)
                {
                    _producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
                }
            }
            return _producer;
        }
    }
}
