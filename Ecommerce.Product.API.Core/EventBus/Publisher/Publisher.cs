using Ecommerce.Product.API.Core.EventBus.Connection;
using Ecommerce.Product.API.Core.Models.Response;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Product.API.Core.EventBus.Publisher
{
    public class Publisher : IPublisher, IDisposable
    {
        #region Properties
        private readonly IConnectionProvider _connectionProvider;
        private readonly string _exchange;
        private readonly string _exchangeType;
        private IModel _channel;
        #endregion

        #region Constructor
        public Publisher(IConnectionProvider connectionProvider, string exchange, string exchangeType)
        {
            _connectionProvider = connectionProvider;
            _exchange = exchange;
            _exchangeType = exchangeType;
            _channel = _connectionProvider.GetConnection().CreateModel();

            CreateConnection();
        }
        #endregion

        #region CreateConnection
        private void CreateConnection()
        {
            try
            {
                _channel.ExchangeDeclare(exchange: _exchange, type: _exchangeType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> RabbitMQ connection error: {ex.Message}");
                throw new Exception("RabbitMQ Connection error");
            }
        }
        #endregion

        #region SendMessage
        private void SendMessage(string message, string routeKey)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(_exchange, routeKey, null, body);
        }
        #endregion

        #region PublishCreatedOrderDetailStockUpdated
        public void PublishCreatedOrderDetailStockUpdated(ProductMessageResponseModel productMessage)
        {
            if (productMessage is null)
                throw new ArgumentException("Product Message cannot be null");

            var message = JsonSerializer.Serialize(productMessage);

            if (_connectionProvider.GetConnection().IsOpen)
                SendMessage(message, "product_stock_changed_order_detail_created");
        }
        #endregion

        #region PublishUpdateOrderDetailStockUpdated
        public void PublishUpdateOrderDetailStockUpdated(ProductMessageResponseModel productMessage)
        {
            if (productMessage is null)
                throw new ArgumentException("Product Message cannot be null");

            var message = JsonSerializer.Serialize(productMessage);

            if (_connectionProvider.GetConnection().IsOpen)
                SendMessage(message, "product_stock_changed_order_detail_updated");
        }
        #endregion

        #region PublishOrderDetailDeletedStockUpdated
        public void PublishOrderDetailDeletedStockUpdated(ProductMessageResponseModel productMessage)
        {
            if (productMessage is null)
                throw new ArgumentException("Product Message cannot be null");

            var message = JsonSerializer.Serialize(productMessage);

            if (_connectionProvider.GetConnection().IsOpen)
                SendMessage(message, "product_stock_changed_order_detail_deleted");
        } 
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (_connectionProvider.GetConnection().IsOpen)
            {
                _channel.Close();
                _connectionProvider.GetConnection().Close();
            }
        } 
        #endregion
    }
}
