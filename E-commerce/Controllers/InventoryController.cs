using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController:ControllerBase
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

        [HttpGet("{productId}")]
        public async Task<ActionResult<InventoryDTO>> GetInventoryByProductId(int productId)
        {
            var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
            if (inventory == null)
            {
                return NotFound();
            }
            return Ok(inventory);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> CreateInventory(int productId)
        {
            try
            {
                await _inventoryService.CreateInventoryAsync(productId);
                return CreatedAtAction(nameof(GetInventoryByProductId), new { productId }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDTO updateStockDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedProducts = await _inventoryService.UpdateStockAsync(updateStockDto);

            // Return the updated products as a response
            return Ok(updatedProducts); // Return 200 OK with the list of updated products
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
            return NoContent(); // Return 204 No Content on successful update
        }


    }
}
