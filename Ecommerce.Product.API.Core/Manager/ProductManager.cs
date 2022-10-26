using Ecommerce.Product.API.Core.Infrastructure;
using Ecommerce.Product.API.Core.Models.Domain;

namespace Ecommerce.Product.API.Core.Manager
{
    public class ProductManager : IProductManager
    {
        private readonly IProductDAL _productDAL;

        public ProductManager(IProductDAL productDAL)
        {
            _productDAL = productDAL;
        }

        public async Task<IEnumerable<ProductModel>> GetAllProducts()
        {
            var products = await _productDAL.GetAllProducts();

            return products ?? new List<ProductModel>();
        }

        public async Task<ProductModel> GetProductById(int id)
        {
            var product = await _productDAL.GetProductById(id);

            return product ?? new ProductModel();
        }

        public async Task CreateProduct(ProductModel product)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            await _productDAL.CreateProduct(product);
        }

        public async Task<bool> UpdateProduct(int id, ProductModel product)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (id != product.Id)
                throw new ArgumentException("Id field does not match");

            if (await _productDAL.ProdusctExists(id) is false)
                throw new ArgumentException("Product Not Found");

            return await _productDAL.UpdateProduct(product);
        }

        public async Task<bool> DeleteProduct(int id)
        {
            if (id < 1)
                throw new ArgumentException("Id field is required");

            if (await _productDAL.ProdusctExists(id) is false)
                throw new ArgumentException("Product Not Found");

            return await _productDAL.DeleteProduct(id);
        }
    }
}
