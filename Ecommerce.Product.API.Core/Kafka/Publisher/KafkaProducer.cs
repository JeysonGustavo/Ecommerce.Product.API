using Confluent.Kafka;
using Ecommerce.Product.API.Core.Kafka.Connection;
using Ecommerce.Product.API.Core.Models.Response;
using System.Text.Json;

namespace Ecommerce.Product.API.Core.Kafka.Publisher
{
    public class KafkaProducer : IKafkaProducer
    {
        #region Properties
        private readonly IKafkaConnectionProvider _kafkaConnectionProvider;
        #endregion

        #region Constructor
        public KafkaProducer(IKafkaConnectionProvider kafkaConnectionProvider)
        {
            _kafkaConnectionProvider = kafkaConnectionProvider;
        }
        #endregion

        #region PublishCreatedOrderDetailStockUpdated
        public async Task PublishCreatedOrderDetailStockUpdated(ProductMessageResponseModel productMessage)
        {
            if (productMessage is null)
                throw new ArgumentException("Product Message cannot be null");

            var message = JsonSerializer.Serialize(productMessage);

            await _kafkaConnectionProvider.GetProducer().ProduceAsync("kafka_product_stock_changed_order_detail_created", new Message<Null, string> { Value = message });
        } 
        #endregion
    }
}
