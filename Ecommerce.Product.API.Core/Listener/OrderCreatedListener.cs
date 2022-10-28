using Ecommerce.Product.API.Core.EventBus.Subscriber;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Product.API.Core.Listener
{
    public class OrderCreatedListener : IHostedService
    {
        #region Properties
        private readonly ISubscriber _subscribe;
        #endregion

        #region Constructor
        public OrderCreatedListener(ISubscriber subscribe)
        {
            _subscribe = subscribe;
        }
        #endregion

        #region StartAsync
        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _subscribe.SubscriberNewOrderDetail();

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
