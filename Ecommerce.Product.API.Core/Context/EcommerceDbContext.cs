using Ecommerce.Product.API.Core.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Product.API.Core.Context
{
    public class EcommerceDbContext : DbContext
    {
        public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : base(options)
        {
        }

        public DbSet<ProductModel> Products { get; set; }
    }
}
