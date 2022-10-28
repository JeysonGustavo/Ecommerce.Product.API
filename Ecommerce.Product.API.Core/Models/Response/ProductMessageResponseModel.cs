namespace Ecommerce.Product.API.Core.Models.Response
{
    public class ProductMessageResponseModel
    {
        #region Properties
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Units { get; set; }
        public bool IsSuccess { get; set; }
        #endregion

        #region Constructor
        public ProductMessageResponseModel(int orderId, int productId, int units, bool isSuccess)
        {
            OrderId = orderId;
            ProductId = productId;
            Units = units;
            IsSuccess = isSuccess;
        } 
        #endregion
    }
}
