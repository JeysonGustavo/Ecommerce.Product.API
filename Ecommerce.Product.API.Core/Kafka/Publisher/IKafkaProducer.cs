﻿using Ecommerce.Product.API.Core.Models.Response;

namespace Ecommerce.Product.API.Core.Kafka.Publisher
{
    public interface IKafkaProducer
    {
        Task PublishCreatedOrderDetailStockUpdated(ProductMessageResponseModel productMessage);
    }
}