namespace Ecommerce.Product.API.Core.Models.Response
{
    public class ProductMessageResponseModel
    {
        #region Properties
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Units { get; set; }
        public bool IsSuccess { get; set; }
        public string DatabaseToDoOperation { get; set; }
        #endregion

        #region Constructor
        public ProductMessageResponseModel(int id, int orderId, int productId, int units, bool isSuccess, string databaseToDoOperation)
        {
            Id = id;
            OrderId = orderId;
            ProductId = productId;
            Units = units;
            IsSuccess = isSuccess;
            DatabaseToDoOperation = databaseToDoOperation;
        }
        #endregion
    }
}
