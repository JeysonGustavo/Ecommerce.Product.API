using Ecommerce.Product.API.Core.EventBus.Subscriber;
using Ecommerce.Product.API.Core.Kafka.Consumer;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Product.API.Core.Listener
{
    public class OrderCreatedListener : IHostedService
    {
        #region Properties
        private readonly ISubscriber _subscribe;
        private readonly IKafkaConsumer _kafkaConsumer;
        #endregion

        #region Constructor
        public OrderCreatedListener(ISubscriber subscribe, IKafkaConsumer kafkaConsumer)
        {
            _subscribe = subscribe;
            _kafkaConsumer = kafkaConsumer;
        }
        #endregion

        #region StartAsync
        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _subscribe.InitializeSubscribers();
            _kafkaConsumer.InitializeConsumers();

            return Task.CompletedTask;
        }
        #endregion

        #region StopAsync
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        } 
        #endregion
    }
}
