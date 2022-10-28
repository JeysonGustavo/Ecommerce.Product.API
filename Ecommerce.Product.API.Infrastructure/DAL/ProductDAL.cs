using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.Infrastructure;
using Ecommerce.Product.API.Core.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Product.API.Infrastructure.DAL
{
    public class ProductDAL : IProductDAL
    {
        #region Properties
        private readonly EcommerceDbContext _context;
        #endregion

        #region Constructor
        public ProductDAL(EcommerceDbContext context)
        {
            _context = context;
        }
        #endregion

        #region GetAllProducts
        public async Task<IEnumerable<ProductModel>> GetAllProducts() => await _context.Products.ToListAsync();
        #endregion

        #region GetProductById
        public async Task<ProductModel?> GetProductById(int id) => await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync();
        #endregion

        #region ProdusctExists
        public async Task<bool> ProdusctExists(int id) => await _context.Products.Where(x => x.Id == id).AnyAsync();
        #endregion

        #region CreateProduct
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
        #endregion

        #region UpdateProduct
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
        #endregion

        #region DeleteProduct
        public async Task<bool> DeleteProduct(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var product = await _context.Products.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (product is null)
                    return false;

                _context.Products.Remove(product);
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
        #endregion
    }
}
