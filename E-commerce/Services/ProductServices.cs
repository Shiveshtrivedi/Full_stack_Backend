using E_commerce.Context;
using E_commerce.Utils;
using E_commerce.Models;
using Microsoft.EntityFrameworkCore;
using E_commerce.DTOs;
using Newtonsoft.Json;

namespace E_commerce.Services
{
    public class ProductServices : IProductServices
    {
        private readonly DataContext _context;
        private readonly MQTTService _mqttService;


        public ProductServices(DataContext context, MQTTService mqttService)
        {
            _context = context;
            _mqttService = mqttService;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var product = await _context.Products.Where(p => !p.DeleteFlag).ToListAsync();

            return product;
        }

        public async Task<Product> CreateProductAsync(ProductDTO productDTO, int userId)
        {
            var product = new Product
            {
                ProductName = productDTO.ProductName,
                ProductDescription = productDTO.ProductDescription,
                Image = productDTO.Image,
                Price = productDTO.Price,
                Stock = productDTO.Stock,
                Category = productDTO.Category,
                Rating = productDTO.Rating,
                CostPrice = productDTO.CostPrice,
                SellingPrice = productDTO.SellingPrice,
                UserId = userId
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var history = new AdminHistory
            {
                ActionType = "Add Product",
                Details = $"Product '{product.ProductName}' was added.",
                ActionDate = DateTime.Now,
                UserId = userId,
                ProductId = product.ProductId,
                IsAdminAction = true
            };

            await _context.Histories.AddAsync(history);
            await _context.SaveChangesAsync();

            var productMessage = new
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Stock = product.Stock
            };

            var jsonMessage = JsonConvert.SerializeObject(productMessage);
            await _mqttService.PublishAsync("product/new", jsonMessage);

            return product;
        }

        public async Task<List<Product>> AddProductsAsync(List<ProductDTO> productDtos)
        {
            var products = new List<Product>();

            foreach (var productDto in productDtos)
            {
                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    ProductDescription = productDto.ProductDescription,
                    Image = productDto.Image,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    Category = productDto.Category,
                    UserId = productDto.UserId,
                    Rating = productDto.Rating,
                    CostPrice = productDto.CostPrice,
                    SellingPrice = productDto.SellingPrice
                };

                products.Add(product);
            }

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            foreach (var product in products)
            {
                var history = new AdminHistory
                {
                    ActionType = "Add Product",
                    Details = $"Product '{product.ProductName}' was added.",
                    ActionDate = DateTime.Now,
                    UserId = product.UserId,
                    ProductId = product.ProductId,
                    IsAdminAction = true
                };

                await _context.Histories.AddAsync(history);
            }

            await _context.SaveChangesAsync();

            return products;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            return product;

        }

        public async Task<IEnumerable<Product>> GetProductByUserIdAsync(int userId)
        {
            var products = await _context.Products
                                                  .Where(p => p.UserId == userId)
                                                  .ToListAsync();
            if (products == null)
                return null;

            return products;
        }

        public async Task<Product> UpdateProductAsync(ProductDTO productDto, int id)
        {
            var findProduct = await _context.Products.FindAsync(id);
            var existingUser = await _context.Users.FindAsync(productDto.UserId);

            if (findProduct == null || existingUser == null)
                return null;

            findProduct.ProductName = productDto.ProductName;
            findProduct.ProductDescription = productDto.ProductDescription;
            findProduct.Image = productDto.Image;
            findProduct.Price = productDto.Price;
            findProduct.Stock = productDto.Stock;
            findProduct.Category = productDto.Category;

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == id);

            if (inventory != null)
            {
                inventory.StockAvailable = productDto.Stock;
            }

            await _context.SaveChangesAsync();
            return findProduct;
        }

        public async Task<Product> DeleteProductAsync(int id)
        {
            var findProduct = await _context.Products.FindAsync(id);

            if (findProduct == null)
                return null;

            findProduct.DeleteFlag = true;
            await _context.SaveChangesAsync();

            var history = await _context.Histories.FirstOrDefaultAsync(h => h.ProductId == id);
            if (history != null && history.DeleteFlag)
            {
                await DeleteProductAndHistory(history.HistoryId, id);
            }

            return findProduct;
        }

        private async Task DeleteProductAndHistory(int historyId, int productId)
        {
            var history = await _context.Histories.FindAsync(historyId);
            if (history != null)
            {
                _context.Histories.Remove(history);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
                if (inventory != null)
                {
                    _context.Inventories.Remove(inventory);
                }

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
        }

    }
}
