using Ecommerce.Product.API.Core.Models.Domain;

namespace Ecommerce.Product.API.Core.Infrastructure
{
    public interface IProductDAL
    {
        Task<IEnumerable<ProductModel>> GetAllProducts();

        Task<ProductModel?> GetProductById(int id);

        Task<bool> ProdusctExists(int id);

        Task CreateProduct(ProductModel product);

        Task<bool> UpdateProduct(ProductModel product);

        Task<bool> DeleteProduct(int id);
    }
}
