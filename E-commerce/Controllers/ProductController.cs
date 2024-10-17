using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IProductServices _productServices;

        public ProductController(IProductServices productServices)
        {
            _productServices = productServices;
        }

        [HttpGet("all/fetchProducts")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productServices.GetAllProductsAsync();

            return Ok(products);
        }
        [HttpGet("/api/product/user/{userId}/getProductByUserIdForHistory")]
        public async Task<IActionResult> GetProductForUser(int userId)
        {
            var products = await _productServices.GetProductByUserIdAsync(userId);

            return Ok(products);
        }

        [HttpGet("{id}/getProductByProductId")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var foundProduct = await _productServices.GetProductByIdAsync(id);

            if (foundProduct == null)
                return NotFound();

            return Ok(foundProduct);
        }

        [HttpPost("{userId}/addProductByUserId")]
        public async Task<IActionResult> CreateProduct(ProductDTO productDTO, int userId)
        {
            var createdProduct = await _productServices.CreateProductAsync(productDTO, userId);

            if (productDTO.Rating > 5)
                return BadRequest();

            return CreatedAtAction(nameof(GetProducts), new { id = productDTO.ProductId }, createdProduct);
        }

        [HttpPost("add-multiple-products")]
        public async Task<IActionResult> AddMultipleProducts([FromBody] List<ProductDTO> productDtos)
        {
            if (productDtos == null || !productDtos.Any())
            {
                return BadRequest("Product list cannot be empty.");
            }

            if (productDtos.Any(p => p.UserId <= 0))
            {
                return BadRequest("Invalid UserId in one or more product entries.");
            }

            var addedProducts = await _productServices.AddProductsAsync(productDtos);

            return CreatedAtAction(nameof(GetProducts), new { count = addedProducts.Count }, addedProducts);
        }

        [HttpPut("{id}/updateProduct")]
        public async Task<IActionResult> UpdateProduct(ProductDTO product, int id)
        {
            var foundProduct = await _productServices.UpdateProductAsync(product, id);
            if (foundProduct == null)
            {
                return NotFound();
            }
            return Ok(foundProduct);
        }
        [HttpDelete("{id}/deleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var foundProduct = await _productServices.DeleteProductAsync(id);

            if (foundProduct == null)
                return NotFound();

            return Ok(foundProduct);
        }


    }
}
