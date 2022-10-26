using Ecommerce.Product.API.Core.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Product.API.Core.Context
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        public DbSet<ProductModel> Products { get; set; }
    }
}
