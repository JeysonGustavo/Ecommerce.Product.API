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
        private readonly IProductManager _productManager;
        private readonly IMapper _mapper;

        public ProductController(IProductManager productManager, IMapper mapper)
        {
            _productManager = productManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllProducts()
        {
            var products = await _productManager.GetAllProducts();

            return Ok(_mapper.Map<IEnumerable<ProductResponseModel>>(products));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProductById(int id)
        {
            var product = await _productManager.GetProductById(id);

            if (product is null)
                return NotFound();

            return Ok(_mapper.Map<ProductResponseModel>(product));
        }

        [HttpPost]
        public async Task<ActionResult> CreateProduct(ProductRequestModel requestModel)
        {
            var product = _mapper.Map<ProductModel>(requestModel);

            await _productManager.CreateProduct(product);

            var response = _mapper.Map<ProductResponseModel>(product);

            return CreatedAtAction("GetProductById", new { Id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductRequestModel requestModel)
        {
            try
            {
                var product = _mapper.Map<ProductModel>(requestModel);

                bool response = await _productManager.UpdateProduct(id, product);

                if (response is false)
                {
                    return NotFound();
                }
                else
                {
                    return NoContent();
                }
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var response = await _productManager.DeleteProduct(id);

            if (response is false)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
