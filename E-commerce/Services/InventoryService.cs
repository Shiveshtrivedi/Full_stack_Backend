using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace E_commerce.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly MQTTService _mqttService;

        public InventoryService(DataContext context, IMapper mapper, MQTTService mqttService)
        {
            _context = context;
            _mapper = mapper;
            _mqttService = mqttService;
        }

        public async Task<IEnumerable<InventoryDTO>> GetAllInventoriesAsync()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Product)     
                .ToListAsync();

            return _mapper.Map<IEnumerable<InventoryDTO>>(inventories);
        }

        public async Task<InventoryDTO> GetInventoryByProductIdAsync(int productId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)     
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null)
            {
                return null;       
            }

            var inventoryDto = _mapper.Map<InventoryDTO>(inventory);
            inventoryDto.StockAvailable = inventory.StockAvailable;
            inventoryDto.StockSold = inventory.StockSold;

            return inventoryDto;
        }



        public async Task<List<ProductSaleDTO>> CreateInventoryAsync(int productId)
        {
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (existingInventory != null)
            {
                throw new Exception("Inventory for this product already exists.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            var inventory = new Inventory
            {
                ProductId = productId,
                StockAvailable = product.Stock,      
                StockSold = 0      
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            var productSaleList = new List<ProductSaleDTO>
    {
        new ProductSaleDTO
        {
            ProductId = productId,
            QuantitySold = 0         
        }
    };

            return productSaleList;
        }


        public async Task<List<ProductSaleDTO>> UpdateStockAsync(UpdateStockDTO updateStockDto)
        {
            var updatedProducts = new List<ProductSaleDTO>();

            foreach (var productSale in updateStockDto.Products)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productSale.ProductId);

                if (product == null)
                {
                    throw new Exception($"Product with ID {productSale.ProductId} not found.");
                }

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productSale.ProductId);

                if (inventory == null)
                {
                    throw new Exception($"Inventory not found for Product ID {productSale.ProductId}.");
                }

                if (product.Stock < productSale.QuantitySold || inventory.StockAvailable < productSale.QuantitySold)
                {
                    throw new Exception($"Insufficient stock available for Product ID {productSale.ProductId}.");
                }

                product.Stock -= productSale.QuantitySold;
                inventory.StockSold += productSale.QuantitySold;
                inventory.StockAvailable -= productSale.QuantitySold;

                updatedProducts.Add(new ProductSaleDTO
                {
                    ProductId = productSale.ProductId,
                    QuantitySold = productSale.QuantitySold
                });

                var stockUpdateMessage = new
                {
                    ProductId = product.ProductId,
                    StockAvailable = inventory.StockAvailable,
                    StockSold = inventory.StockSold
                };

                var jsonMessage = JsonConvert.SerializeObject(stockUpdateMessage);
                await _mqttService.PublishAsync("inventory-updates", jsonMessage);       
            }

            await _context.SaveChangesAsync();
            return updatedProducts;
        }




        public async Task<bool> AdminIncreaseStockAsync(AdminUpdateStockDTO updateStockDto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == updateStockDto.ProductId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            product.Stock += updateStockDto.AdditionalStock;      

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == updateStockDto.ProductId);

            if (inventory != null)
            {
                inventory.StockAvailable += updateStockDto.AdditionalStock;     
            }

            await _context.SaveChangesAsync();        
            return true;
        }
    }
}





