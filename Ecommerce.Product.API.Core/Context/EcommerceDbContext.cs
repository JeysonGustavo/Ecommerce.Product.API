using Ecommerce.Product.API.Core.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Product.API.Core.Context
{
    public class EcommerceDbContext : DbContext
    {
        #region Constructor
        public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : base(options)
        {
        }
        #endregion

        #region DbSets
        public DbSet<ProductModel> Products { get; set; } 
        #endregion
    }
}
