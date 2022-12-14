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
            SubscriberOrderDetailDeleted();
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
            var queueName = _channel.QueueDeclare("new_order_detail_created_queue", false, false, false).QueueName;
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

            var productMessage = new ProductMessageResponseModel(orderDetail.Id, orderDetail.OrderId, orderDetail.ProductId, orderDetail.Units, false, "Update product to deduct units or delete this order detail and do not change the AvailableStock on Product table");

            try
            {
                var product = _context.Products.Where(x => x.Id == orderDetail.ProductId).FirstOrDefault();

                if (product is null || product.MaxStockThreshold < orderDetail.Units)
                    throw new ArgumentException("Product not found or stock insufficient");

                product.AvailableStock = product.AvailableStock - orderDetail.Units;
                //throw new Exception();
                _context.Entry(product).State = EntityState.Modified;
                productMessage.IsSuccess = _context.SaveChanges() > 0;
                productMessage.DatabaseToDoOperation = productMessage.IsSuccess ? string.Empty : productMessage.DatabaseToDoOperation;

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
            var queueName = _channel.QueueDeclare("update_order_detail_units_queue", false, false, false).QueueName;
            _channel.QueueBind(queueName, _exchange, "update_order_detail_units");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += UpdateOrderDetailUnitsMessageReceived;

            _channel.BasicConsume(queueName, true, consumer);
        }
        #endregion

        #region UpdateOrderDetailUnitsMessageReceived
        private void UpdateOrderDetailUnitsMessageReceived(object? sender, BasicDeliverEventArgs args)
        {
            var message = GetMessage(args);

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Could not receive the message from Order service");

            var updateOrderDetailUnits = JsonSerializer.Deserialize<OrderDetailUpdateUnitsResponseModel>(message);

            if (updateOrderDetailUnits is null)
                throw new ArgumentException("Could not receive the message from Order service");

            var productMessage = new ProductMessageResponseModel(updateOrderDetailUnits.Id, updateOrderDetailUnits.OrderId, updateOrderDetailUnits.ProductId, (updateOrderDetailUnits.OldUnits - updateOrderDetailUnits.NewUnits), false, "Update product to deduct or add units");

            try
            {
                var product = _context.Products.Where(x => x.Id == updateOrderDetailUnits.ProductId).FirstOrDefault();

                if (product is null || product.MaxStockThreshold < updateOrderDetailUnits.NewUnits)
                    throw new ArgumentException("Product not found or stock insufficient");

                product.AvailableStock = product.AvailableStock + updateOrderDetailUnits.OldUnits - updateOrderDetailUnits.NewUnits;
                //throw new Exception();
                _context.Entry(product).State = EntityState.Modified;
                productMessage.IsSuccess = _context.SaveChanges() > 0;
                productMessage.DatabaseToDoOperation = productMessage.IsSuccess ? string.Empty : productMessage.DatabaseToDoOperation;

                _publisher.PublishUpdateOrderDetailStockUpdated(productMessage);
            }
            catch (ArgumentException)
            {
                _publisher.PublishUpdateOrderDetailStockUpdated(productMessage);
                throw;
            }
            catch (Exception)
            {
                _publisher.PublishUpdateOrderDetailStockUpdated(productMessage);
                throw;
            }
        }
        #endregion

        #region SubscriberOrderDetailDeleted
        private void SubscriberOrderDetailDeleted()
        {
            var queueName = _channel.QueueDeclare("order_detail_deleted_queue", false, false, false).QueueName;
            _channel.QueueBind(queueName, _exchange, "order_detail_deleted");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OrderDetailDeletedMessageReceived;

            _channel.BasicConsume(queueName, true, consumer);
        }
        #endregion

        #region OrderDetailDeletedMessageReceived
        private void OrderDetailDeletedMessageReceived(object? sender, BasicDeliverEventArgs args)
        {
            var message = GetMessage(args);

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Could not receive the message from Order service");

            var orderDetail = JsonSerializer.Deserialize<OrderDetailModel>(message);

            if (orderDetail is null)
                throw new ArgumentException("Could not receive the message from Order service");

            var productMessage = new ProductMessageResponseModel(orderDetail.Id, orderDetail.OrderId, orderDetail.ProductId, orderDetail.Units, false, "Insert order detail and Update product to deduct units");

            try
            {
                var product = _context.Products.Where(x => x.Id == orderDetail.ProductId).FirstOrDefault();

                if (product is null || product.MaxStockThreshold < orderDetail.Units)
                    throw new ArgumentException("Product not found or stock insufficient");

                product.AvailableStock = product.AvailableStock + orderDetail.Units;
                //throw new Exception();
                _context.Entry(product).State = EntityState.Modified;
                productMessage.IsSuccess = _context.SaveChanges() > 0;
                productMessage.DatabaseToDoOperation = productMessage.IsSuccess ? string.Empty : productMessage.DatabaseToDoOperation;

                _publisher.PublishOrderDetailDeletedStockUpdated(productMessage);
            }
            catch (ArgumentException)
            {
                _publisher.PublishOrderDetailDeletedStockUpdated(productMessage);
                throw;
            }
            catch (Exception)
            {
                _publisher.PublishOrderDetailDeletedStockUpdated(productMessage);
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
