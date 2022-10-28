using Ecommerce.Product.API.Core.Models.Response;

namespace Ecommerce.Product.API.Core.EventBus.Publisher
{
    public interface IPublisher
    {
        void PublishCreatedOrderDetailStockUpdated(ProductMessageResponseModel productMessage);

        void PublishUpdateOrderDetailStockUpdated(bool isSuccess);
    }
}
