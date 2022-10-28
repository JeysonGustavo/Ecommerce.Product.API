using AutoMapper;
using Ecommerce.Product.API.Core.Manager;
using Ecommerce.Product.API.Core.Models.Domain;
using Ecommerce.Product.API.Core.Models.Request;
using Ecommerce.Product.API.Core.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Product.API.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        #region Properties
        private readonly IProductManager _productManager;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public ProductController(IProductManager productManager, IMapper mapper)
        {
            _productManager = productManager;
            _mapper = mapper;
        }
        #endregion

        #region GetAllProducts
        [HttpGet]
        public async Task<ActionResult> GetAllProducts()
        {
            var products = await _productManager.GetAllProducts();

            return Ok(_mapper.Map<IEnumerable<ProductResponseModel>>(products));
        }
        #endregion

        #region GetProductById
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProductById(int id)
        {
            var product = await _productManager.GetProductById(id);

            if (product is null)
                return NotFound();

            return Ok(_mapper.Map<ProductResponseModel>(product));
        }
        #endregion

        #region CreateProduct
        [HttpPost]
        public async Task<ActionResult> CreateProduct(ProductRequestModel requestModel)
        {
            try
            {
                var product = _mapper.Map<ProductModel>(requestModel);

                await _productManager.CreateProduct(product);

                var response = _mapper.Map<ProductResponseModel>(product);

                return CreatedAtAction("GetProductById", new { Id = response.Id }, response);
            }
            catch (ArgumentException ae)
            {
                return Ok(ae.Message);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        #endregion

        #region UpdateProduct
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductRequestModel requestModel)
        {
            try
            {
                var product = _mapper.Map<ProductModel>(requestModel);

                bool response = await _productManager.UpdateProduct(id, product);

                return Ok(response);
            }
            catch (ArgumentException ae)
            {
                return Ok(ae.Message);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        #endregion

        #region DeleteProduct
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var response = await _productManager.DeleteProduct(id);

                return Ok(response);
            }
            catch (ArgumentException ae)
            {
                return Ok(ae.Message);
            }
            catch (Exception)
            {
                return NotFound();
            }
        } 
        #endregion
    }
}
