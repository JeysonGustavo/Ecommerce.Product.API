namespace Ecommerce.Product.API.Core.Models.Response
{
    public class OrderDetailUpdateUnitsResponseModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int OldUnits { get; set; }
        public int NewUnits { get; set; }
    }
}
