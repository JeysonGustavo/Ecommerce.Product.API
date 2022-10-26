using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Product.API.Core.Models.Domain
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        [Required]
        public int AvailableStock { get; set; }

        [Required]
        public int MaxStockThreshold { get; set; }
    }
}
