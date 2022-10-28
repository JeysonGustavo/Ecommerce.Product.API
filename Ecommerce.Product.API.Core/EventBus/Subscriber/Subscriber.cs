using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.EventBus.Connection;
using Ecommerce.Product.API.Core.EventBus.Publisher;
using Ecommerce.Product.API.Core.Models.Domain;
using Ecommerce.Product.API.Core.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Product.API.Core.EventBus.Subscriber
{
    public class Subscriber : ISubscriber, IDisposable
    {
        #region Properties
        private readonly IPublisher _publisher;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EcommerceDbContext _context;
        private readonly IServiceScope _scope;
        private readonly string _exchange;
        private readonly string _exchangeType;
        private IModel _channel;
        #endregion

        #region Constructor
        public Subscriber(IConnectionProvider connectionProvider, IServiceScopeFactory scopeFactory, string exchange, string exchangeType)
        {
            _scopeFactory = scopeFactory;
            _scope = _scopeFactory.CreateScope();

            _connectionProvider = connectionProvider;
            _publisher = _scope.ServiceProvider.GetRequiredService<IPublisher>();
            _context = _scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
            _exchange = exchange;
            _exchangeType = exchangeType;
            _channel = _connectionProvider.GetConnection().CreateModel();

            CreateConnection();
        }
        #endregion

        #region InitializeSubscribers
        public void InitializeSubscribers()
        {
            SubscriberNewOrderDetail();
            SubscribeUpdateOrderDetailUnits();
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

        #region GetMessage
        private string GetMessage(BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            return message;
        }
        #endregion

        #region SubscriberNewOrderDetail
        private void SubscriberNewOrderDetail()
        {
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, _exchange, "new_order_detail_created");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += NewOrderDetailMessageReceived;

            _channel.BasicConsume(queueName, true, consumer);
        }
        #endregion

        #region NewOrderDetailMessageReceived
        private void NewOrderDetailMessageReceived(object? sender, BasicDeliverEventArgs args)
        {
            var message = GetMessage(args);

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Could not receive the message from Order service");

            var orderDetail = JsonSerializer.Deserialize<OrderDetailModel>(message);

            if (orderDetail is null)
                throw new ArgumentException("Could not receive the message from Order service");

            var productMessage = new ProductMessageResponseModel(orderDetail.OrderId, orderDetail.ProductId, orderDetail.Units, false);

            try
            {
                var product = _context.Products.Where(x => x.Id == orderDetail.ProductId).FirstOrDefault();

                if (product is null || product.MaxStockThreshold < orderDetail.Units)
                    throw new ArgumentException("Product not found or stock insufficient");

                product.AvailableStock = product.AvailableStock - orderDetail.Units;
                _context.Entry(product).State = EntityState.Modified;
                productMessage.IsSuccess = _context.SaveChanges() > 0;
                productMessage.IsSuccess = false;

                _publisher.PublishCreatedOrderDetailStockUpdated(productMessage);
            }
            catch (ArgumentException)
            {
                _publisher.PublishCreatedOrderDetailStockUpdated(productMessage);
                throw;
            }
            catch (Exception)
            {
                _publisher.PublishCreatedOrderDetailStockUpdated(productMessage);
                throw;
            }
        }
        #endregion

        #region SubscribeUpdateOrderDetailUnits
        private void SubscribeUpdateOrderDetailUnits()
        {
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, _exchange, "update_order_detail_units");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += UpdateOrderDetailUnitsMessageReceived;

            _channel.BasicConsume(queueName, true, consumer);
        }
        #endregion

        #region UpdateOrderDetailUnitsMessageReceived
        private void UpdateOrderDetailUnitsMessageReceived(object? sender, BasicDeliverEventArgs args)
        {
            try
            {
                var message = GetMessage(args);

                var updateOrderDetailUnits = JsonSerializer.Deserialize<OrderDetailUpdateUnitsResponseModel>(message);

                if (updateOrderDetailUnits is null)
                    throw new ArgumentException("Could not receive the message from Order service");

                var product = _context.Products.Where(x => x.Id == updateOrderDetailUnits.ProductId).FirstOrDefault();

                if (product is null || product.MaxStockThreshold < updateOrderDetailUnits.NewUnits)
                    throw new ArgumentException("Product not found or stock insufficient");

                product.AvailableStock = product.AvailableStock + updateOrderDetailUnits.OldUnits - updateOrderDetailUnits.NewUnits;
                _context.Entry(product).State = EntityState.Modified;
                bool isSuccess = _context.SaveChanges() > 0;

                if (isSuccess is true)
                    _publisher.PublishUpdateOrderDetailStockUpdated(true);
                else
                    _publisher.PublishUpdateOrderDetailStockUpdated(false);
            }
            catch (ArgumentException)
            {
                _publisher.PublishUpdateOrderDetailStockUpdated(false);
                throw;
            }
            catch (Exception)
            {
                _publisher.PublishUpdateOrderDetailStockUpdated(false);
                throw;
            }
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
