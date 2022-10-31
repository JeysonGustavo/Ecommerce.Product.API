using Confluent.Kafka;

namespace Ecommerce.Product.API.Core.Kafka.Connection
{
    public class KafkaConnectionProvider : IKafkaConnectionProvider
    {
        #region Properties
        private readonly IProducer<Null, string> _producerBuilder;
        private readonly IConsumer<Null, string> _consumerBuilder;
        #endregion

        #region Constructor
        public KafkaConnectionProvider(ProducerConfig producerConfig, ConsumerConfig consumerConfig)
        {
            _producerBuilder = new ProducerBuilder<Null, string>(producerConfig).Build();
            _consumerBuilder = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        }
        #endregion

        #region GetProducer
        public IProducer<Null, string> GetProducer() => _producerBuilder;
        #endregion

        #region GetConsumer
        public IConsumer<Null, string> GetConsumer() => _consumerBuilder; 
        #endregion
    }
}
