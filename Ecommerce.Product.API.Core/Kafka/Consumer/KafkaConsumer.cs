using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.Kafka.Connection;
using Ecommerce.Product.API.Core.Kafka.Publisher;
using Ecommerce.Product.API.Core.Models.Domain;
using Ecommerce.Product.API.Core.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Ecommerce.Product.API.Core.Kafka.Consumer
{
    public class KafkaConsumer : IKafkaConsumer
    {
        #region Properties
        private readonly IKafkaConnectionProvider _kafkaConnectionProvider;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceScope _scope;
        private readonly EcommerceDbContext _context;
        public CancellationTokenSource _cancellationToken = new();
        #endregion

        #region Constructor
        public KafkaConsumer(IKafkaConnectionProvider kafkaConnectionProvider, IKafkaProducer kafkaProducer, IServiceScopeFactory scopeFactory)
        {
            _kafkaConnectionProvider = kafkaConnectionProvider;
            _kafkaProducer = kafkaProducer;

            _scopeFactory = scopeFactory;
            _scope = _scopeFactory.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        }
        #endregion

        #region InitializeConsumers
        public void InitializeConsumers()
        {
            List<string> topics = new List<string>();
            topics.Add("kafka_new_order_detail_created");
            topics.Add("kafka_updated_order_detail_units");
            topics.Add("kafka_order_detail_deleted");
            SubscribeTopics(topics);
        }
        #endregion

        #region SubscribeTopics
        private async void SubscribeTopics(List<string> topics)
        {
            await Task.Run(() =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        _kafkaConnectionProvider.GetConsumer().Subscribe(topics);

                        var response = _kafkaConnectionProvider.GetConsumer().Consume(_cancellationToken.Token);

                        if (response is not null)
                        {
                            switch (response.Topic)
                            {
                                case "kafka_new_order_detail_created":
                                    NewOrderDetailMessageReceived(response.Message.Value);
                                    break;

                                case "kafka_updated_order_detail_units":
                                    UpdateOrderDetailUnitsMessageReceived(response.Message.Value);
                                    break;

                                case "kafka_order_detail_deleted":
                                    OrderDetailDeletedMessageReceived(response.Message.Value);
                                    break;

                                default:
                                    Console.WriteLine($"--> Missing Topic, name: {response.Topic}");
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--> Exception receiving product message, Error: {ex.Message}");
                    }
                }
            });
        } 
        #endregion

        #region NewOrderDetailMessageReceived
        private async void NewOrderDetailMessageReceived(string message)
        {
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
                productMessage.IsSuccess = await _context.SaveChangesAsync() > 0;
                productMessage.DatabaseToDoOperation = productMessage.IsSuccess ? string.Empty : productMessage.DatabaseToDoOperation;

                await _kafkaProducer.PublishCreatedOrderDetailStockUpdated(productMessage);
            }
            catch (ArgumentException aex)
            {
                Console.WriteLine($"--> Kafka Consumer New Order Detail Argument Exception: {aex.Message}");
                await _kafkaProducer.PublishCreatedOrderDetailStockUpdated(productMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Kafka Consumer New Order Detail Exception: {ex.Message}");
                await _kafkaProducer.PublishCreatedOrderDetailStockUpdated(productMessage);
            }
        }
        #endregion

        #region UpdateOrderDetailUnitsMessageReceived
        private async void UpdateOrderDetailUnitsMessageReceived(string message)
        {
            var updateOrderDetailUnits = JsonSerializer.Deserialize<OrderDetailUpdateUnitsResponseModel>(message);

            if (updateOrderDetailUnits is null)
                throw new ArgumentException("Could not receive the message from Order service");

            var productMessage = new ProductMessageResponseModel(updateOrderDetailUnits.Id, updateOrderDetailUnits.OrderId, updateOrderDetailUnits.ProductId, updateOrderDetailUnits.OldUnits, false, "Update product to deduct or add units");

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

                await _kafkaProducer.PublishUpdateOrderDetailStockUpdated(productMessage);
            }
            catch (ArgumentException aex)
            {
                Console.WriteLine($"--> Kafka Consumer Update Order Detail Argument Exception: {aex.Message}");
                await _kafkaProducer.PublishUpdateOrderDetailStockUpdated(productMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Kafka Consumer Update Order Detail Exception: {ex.Message}");
                await _kafkaProducer.PublishUpdateOrderDetailStockUpdated(productMessage);
            }
        }
        #endregion

        #region OrderDetailDeletedMessageReceived
        private async void OrderDetailDeletedMessageReceived(string message)
        {
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

                await _kafkaProducer.PublishOrderDetailDeletedStockUpdated(productMessage);
            }
            catch (ArgumentException aex)
            {
                Console.WriteLine($"--> Kafka Consumer Delete Order Detail Argument Exception: {aex.Message}");
                await _kafkaProducer.PublishOrderDetailDeletedStockUpdated(productMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Kafka Consumer Delete Order Detail Exception: {ex.Message}");
                await _kafkaProducer.PublishOrderDetailDeletedStockUpdated(productMessage);
            }
        }
        #endregion
    }
}
