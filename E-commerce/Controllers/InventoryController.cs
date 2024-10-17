using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDTO>>> GetAllInventories()
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(inventories);
        }

        [HttpGet("{productId}/getInventory")]
        public async Task<ActionResult<InventoryDTO>> GetInventoryByProductId(int productId)
        {
            var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
            if (inventory == null)
            {
                return NotFound();
            }
            return Ok(inventory);
        }

        [HttpPost("{productId}/createInventory")]
        public async Task<IActionResult> CreateInventory(int productId)
        {
            try
            {
                var createdInventory = await _inventoryService.CreateInventoryAsync(productId);

                return CreatedAtAction(nameof(GetInventoryByProductId), new { productId }, createdInventory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("updateStock")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDTO updateStockDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedProducts = await _inventoryService.UpdateStockAsync(updateStockDto);

            return Ok(updatedProducts);
        }


        [HttpPut("admin/increase-stock")]
        public async Task<IActionResult> AdminIncreaseStock([FromBody] AdminUpdateStockDTO updateStockDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _inventoryService.AdminIncreaseStockAsync(updateStockDto);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }


    }
}
