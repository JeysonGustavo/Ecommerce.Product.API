using Ecommerce.Product.API.Core.Models.Response;

namespace Ecommerce.Product.API.Core.EventBus.Publisher
{
    public interface IPublisher
    {
        void PublishCreatedOrderDetailStockUpdated(ProductMessageResponseModel productMessage);

        void PublishUpdateOrderDetailStockUpdated(ProductMessageResponseModel productMessage);

        void PublishOrderDetailDeletedStockUpdated(ProductMessageResponseModel productMessage);
    }
}
