using E_commerce.Context;
using E_commerce.Utils;
using E_commerce.Models;
using Microsoft.EntityFrameworkCore;
using E_commerce.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Razorpay.Api;

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

        public async Task<IEnumerable<Models.Product>> GetAllProductsAsync()
        {
            try
            {
                return await _context.Products.Where(p => !p.DeleteFlag).ToListAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while retrieving products from database.", ex);
            }
        }

        public async Task<Models.Product> CreateProductAsync(ProductDTO productDTO, int userId)
        {
            try
            {
                var product = new Models.Product
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
                    productId = product.ProductId,
                    productName = productDTO.ProductName,
                    productDescription = productDTO.ProductDescription,
                    image = productDTO.Image,
                    price = productDTO.Price,
                    stock = productDTO.Stock,
                    category = productDTO.Category,
                    rating = productDTO.Rating,
                    costPrice = productDTO.CostPrice,
                    sellingPrice = productDTO.SellingPrice,
                    userId = userId
                };

                var jsonMessage = JsonConvert.SerializeObject(productMessage);
                await _mqttService.PublishAsync("product/new", jsonMessage);

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the product.", ex);
            }
        }

        public async Task<List<Models.Product>> AddProductsAsync(List<ProductDTO> productDtos)
        {
            try
            {
                var products = new List<Models.Product>();

                foreach (var productDto in productDtos)
                {
                    var product = new Models.Product
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
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding products.", ex);
            }
        }

        public async Task<Models.Product> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                return product;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the product with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Models.Product>> GetProductByUserIdAsync(int userId)
        {
            try
            {
                var products = await _context.Products
                                              .Where(p => p.UserId == userId)
                                              .ToListAsync();
                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving products for user ID {userId}.", ex);
            }
        }

        public async Task<Models.Product> UpdateProductAsync(ProductDTO productDto, int id)
        {
            try
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
                var productMessage = new
                {
                    productId = findProduct.ProductId,
                    productName = findProduct.ProductName,
                    productDescription = findProduct.ProductDescription,
                    price = findProduct.Price,
                    stock = findProduct.Stock,
                    category = findProduct.Category,
                };

                var jsonMessage = JsonConvert.SerializeObject(productMessage);
                await _mqttService.PublishAsync("product/update", jsonMessage);

                return findProduct;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the product with ID {id}.", ex);
            }
        }

        public async Task<Models.Product> DeleteProductAsync(int id)
        {
            try
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

                var productMessage = new
                {
                    productId = findProduct.ProductId,
                    productName = findProduct.ProductName,
                    productDescription = findProduct.ProductDescription,
                    price = findProduct.Price,
                    stock = findProduct.Stock,
                    category = findProduct.Category,
                };

                var jsonMessage = JsonConvert.SerializeObject(productMessage);
                await _mqttService.PublishAsync("product/delete", jsonMessage);


                return findProduct;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the product with ID {id}.", ex);
            }
        }

        private async Task DeleteProductAndHistory(int historyId, int productId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting product and its history.", ex);
            }
        }
    }
}
