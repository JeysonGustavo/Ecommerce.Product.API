namespace Ecommerce.Product.API.Core.Models.Domain
{
    public class OrderDetailModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Units { get; set; }
    }
}
