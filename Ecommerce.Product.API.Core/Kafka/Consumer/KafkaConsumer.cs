using Confluent.Kafka;
using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.Kafka.Connection;
using Ecommerce.Product.API.Core.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ecommerce.Product.API.Core.Kafka.Consumer
{
    public class KafkaConsumer : IKafkaConsumer
    {
        #region Properties
        private readonly IKafkaConnectionProvider _kafkaConnectionProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceScope _scope;
        private readonly EcommerceDbContext _context;
        public CancellationTokenSource _cancellationToken;
        #endregion

        #region Constructor
        public KafkaConsumer(IKafkaConnectionProvider kafkaConnectionProvider, IServiceScopeFactory scopeFactory)
        {
            _kafkaConnectionProvider = kafkaConnectionProvider;
            _scopeFactory = scopeFactory;
            _scope = _scopeFactory.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        }
        #endregion

        public void InitializeConsumers()
        {
            ConsumerNewOrderDetail();
        }

        #region ConsumerNewOrderDetail
        private void ConsumerNewOrderDetail()
        {
            Task.Run(() => {
                _cancellationToken = new();

                try
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        _kafkaConnectionProvider.GetConsumer().Subscribe("kafka_product_stock_changed_order_detail_created");
                        var response = _kafkaConnectionProvider.GetConsumer().Consume(_cancellationToken.Token);

                        if (response is not null)
                        {
                            var orderDetail = JsonSerializer.Deserialize<OrderDetailModel>(response.Message.Value);

                            if (orderDetail is null)
                                throw new ArgumentException("Could not receive the message from Order service");

                            var product = _context.Products.Where(x => x.Id == orderDetail.ProductId).FirstOrDefault();

                            if (product is null || product.MaxStockThreshold < orderDetail.Units)
                                throw new ArgumentException("Product not found or stock insufficient");

                            product.AvailableStock = product.AvailableStock - orderDetail.Units;
                            //throw new Exception();
                            _context.Entry(product).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Kafka Consumer New Order Detail Error: {ex.Message}");
                }

            });
        }
        #endregion
    }
}
