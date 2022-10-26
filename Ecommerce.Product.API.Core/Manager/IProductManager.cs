using Ecommerce.Product.API.Core.Models.Domain;

namespace Ecommerce.Product.API.Core.Manager
{
    public interface IProductManager
    {
        Task<IEnumerable<ProductModel>> GetAllProducts();

        Task<ProductModel> GetProductById(int id);

        Task CreateProduct(ProductModel product);

        Task<bool> UpdateProduct(int id, ProductModel product);

        Task<bool> DeleteProduct(int id);
    }
}
