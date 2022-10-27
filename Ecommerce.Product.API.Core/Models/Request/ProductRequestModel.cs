namespace Ecommerce.Product.API.Core.Models.Request
{
    public class ProductRequestModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }
        public int MaxStockThreshold { get; set; }
    }
}
