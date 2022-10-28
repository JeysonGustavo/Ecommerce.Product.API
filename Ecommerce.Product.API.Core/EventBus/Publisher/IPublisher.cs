namespace Ecommerce.Product.API.Core.EventBus.Publisher
{
    public interface IPublisher
    {
        void PublishProductStock(bool isSuccess);
    }
}
