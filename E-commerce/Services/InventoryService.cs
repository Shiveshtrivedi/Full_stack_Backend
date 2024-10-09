using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public InventoryService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventoryDTO>> GetAllInventoriesAsync()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Product) // Include related product information
                .ToListAsync();

            return _mapper.Map<IEnumerable<InventoryDTO>>(inventories);
        }

        public async Task<InventoryDTO> GetInventoryByProductIdAsync(int productId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product) // Include related product information
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null)
            {
                return null; // Or throw an exception if preferred
            }

            var inventoryDto = _mapper.Map<InventoryDTO>(inventory);
            // Map additional fields if necessary
            inventoryDto.StockAvailable = inventory.StockAvailable;
            inventoryDto.StockSold = inventory.StockSold;

            return inventoryDto;
        }

        // New method to create inventory using initial stock from product
        public async Task<bool> CreateInventoryAsync(int productId)
        {
            // Check if inventory already exists
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (existingInventory != null)
            {
                throw new Exception("Inventory for this product already exists.");
            }

            // Get product to retrieve its stock value
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            // Create a new inventory entry using product's stock value
            var inventory = new Inventory
            {
                ProductId = productId,
                StockAvailable = product.Stock, // Initialize with the product's stock
                StockSold = 0 // Start with zero sold stock
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return true; // Successfully created inventory
        }


        public async Task<List<ProductSaleDTO>> UpdateStockAsync(UpdateStockDTO updateStockDto)
        {
            var updatedProducts = new List<ProductSaleDTO>();

            foreach (var productSale in updateStockDto.Products)
            {
                Console.WriteLine($"Updating stock for Product ID: {productSale.ProductId}");

                // Find the product by its ID
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productSale.ProductId);

                if (product == null)
                {
                    throw new Exception($"Product with ID {productSale.ProductId} not found.");
                }

                // Check if the inventory exists for this product
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productSale.ProductId);

                if (inventory == null)
                {
                    throw new Exception($"Inventory not found for Product ID {productSale.ProductId}.");
                }

                // Check if the stock is sufficient for the quantity sold
                if (product.Stock < productSale.QuantitySold || inventory.StockAvailable < productSale.QuantitySold)
                {
                    throw new Exception($"Insufficient stock available for Product ID {productSale.ProductId}.");
                }

                // Update the product stock
                product.Stock -= productSale.QuantitySold; // Deduct sold quantity

                // Update the inventory stock
                inventory.StockSold += productSale.QuantitySold; // Increment stock sold
                inventory.StockAvailable -= productSale.QuantitySold; // Deduct from available stock

                Console.WriteLine($"Product ID: {productSale.ProductId} - Stock updated.");

                // Add the updated product to the result list
                updatedProducts.Add(new ProductSaleDTO
                {
                    ProductId = productSale.ProductId,
                    QuantitySold = productSale.QuantitySold
                });
            }

            // Save changes after processing all products
            await _context.SaveChangesAsync();

            // Return the list of updated products
            return updatedProducts;
        }

        //public async Task<bool> UpdateStockAsync(UpdateStockDTO updateStockDto)
        //{
        //    foreach (var productSale in updateStockDto.Products)
        //    {
        //        Console.WriteLine($"Updating stock for Product ID: {productSale.ProductId}");

        //        // Find the product by its ID
        //        var product = await _context.Products
        //            .FirstOrDefaultAsync(p => p.ProductId == productSale.ProductId);

        //        if (product == null)
        //        {
        //            throw new Exception($"Product with ID {productSale.ProductId} not found.");
        //        }

        //        // Check if the inventory exists for this product
        //        var inventory = await _context.Inventories
        //            .FirstOrDefaultAsync(i => i.ProductId == productSale.ProductId);

        //        if (inventory == null)
        //        {
        //            throw new Exception($"Inventory not found for Product ID {productSale.ProductId}.");
        //        }

        //        // Check if the stock is sufficient for the quantity sold
        //        if (product.Stock < productSale.QuantitySold || inventory.StockAvailable < productSale.QuantitySold)
        //        {
        //            throw new Exception($"Insufficient stock available for Product ID {productSale.ProductId}.");
        //        }

        //        // Update the product stock
        //        product.Stock -= productSale.QuantitySold; // Deduct sold quantity

        //        // Update the inventory stock
        //        inventory.StockSold += productSale.QuantitySold; // Increment stock sold
        //        inventory.StockAvailable -= productSale.QuantitySold; // Deduct from available stock

        //        Console.WriteLine($"Product ID: {productSale.ProductId} - Stock updated.");
        //    }

        //    // Save changes after processing all products
        //    await _context.SaveChangesAsync();
        //    return true;
        //}


        public async Task<bool> AdminIncreaseStockAsync(AdminUpdateStockDTO updateStockDto)
        {
            // Find the product by its ID
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == updateStockDto.ProductId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            // Update the product stock by adding the new stock
            product.Stock += updateStockDto.AdditionalStock;

            // If inventory management is used, sync it with the inventory table as well
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == updateStockDto.ProductId);

            if (inventory != null)
            {
                inventory.StockAvailable += updateStockDto.AdditionalStock;
            }

            // Save changes
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
