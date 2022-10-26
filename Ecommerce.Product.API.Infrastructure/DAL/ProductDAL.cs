using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.Infrastructure;
using Ecommerce.Product.API.Core.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Product.API.Infrastructure.DAL
{
    public class ProductDAL : IProductDAL
    {
        private readonly ProductDbContext _context;

        public ProductDAL(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductModel>> GetAllProducts() => await _context.Products.ToListAsync();

        public async Task<ProductModel?> GetProductById(int id) => await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<bool> ProdusctExists(int id) => await _context.Products.Where(x => x.Id == id).AnyAsync();

        public async Task CreateProduct(ProductModel product)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                if (product is null)
                    throw new ArgumentNullException(nameof(product));

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public async Task<bool> UpdateProduct(ProductModel product)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.Entry(product).State = EntityState.Modified;

                bool isSuccess = await _context.SaveChangesAsync() > 0;

                if (isSuccess)
                    transaction.Commit();
                else
                    transaction.Rollback();

                return isSuccess;
            }
            catch (Exception)
            {
                transaction.Rollback();

                return false;
            }
        }

        public async Task<bool> DeleteProduct(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var product = await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (product is null)
                    return false;

                _context.Products.Remove(product);
                bool isSuccess =  await _context.SaveChangesAsync() > 0;

                if (isSuccess)
                    transaction.Commit();
                else
                    transaction.Rollback();

                return isSuccess;
            }
            catch (Exception)
            {
                transaction.Rollback();

                return false;
            }
        }        
    }
}
