using RabbitMQ.Client;

namespace Ecommerce.Product.API.Core.EventBus.Connection
{
    public interface IConnectionProvider : IDisposable
    {
        IConnection GetConnection();
    }
}
