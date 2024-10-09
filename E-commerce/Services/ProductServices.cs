using E_commerce.Context;
using E_commerce.Utils;
using E_commerce.Models;
using Microsoft.EntityFrameworkCore;
using E_commerce.DTOs;

namespace E_commerce.Services
{
    public class ProductServices : IProductServices
    {
        private readonly DataContext _context;
        
        public ProductServices(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var product = await _context.Products.ToListAsync();

            return  product;
        }

        public async Task<Product> CreateProductAsync(ProductDTO productDTO,int userId)
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
                    Rating = productDto.Rating

                };

                products.Add(product);
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

            await _context.Products.AddRangeAsync(products);
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
            if(products == null)
                return null;

            return products;
        }

        public async Task<Product> UpdateProductAsync(ProductDTO productDto,int id)
        {
            var findProduct = await _context.Products.FindAsync(id);
            var existingUser = await _context.Users.FindAsync(productDto.UserId);
            Console.WriteLine("existing user "+existingUser,findProduct);
            Console.WriteLine("existing user "+findProduct);
            if (findProduct == null || existingUser == null)
                return null;

            findProduct.ProductName = productDto.ProductName;
            findProduct.ProductDescription = productDto.ProductDescription;
            findProduct.Image = productDto.Image;
            findProduct.Price = productDto.Price;
            findProduct.Stock = productDto.Stock;
            findProduct.Category = productDto.Category;

            await _context.SaveChangesAsync();
            return findProduct;
        }

        public async Task<Product> DeleteProductAsync(int id)
        {
            var findProduct = await _context.Products.FindAsync(id);
            
            if (findProduct == null)
                return null;

            _context.Products.Remove(findProduct);
            await _context.SaveChangesAsync();
            return findProduct;
        }
    }
}
