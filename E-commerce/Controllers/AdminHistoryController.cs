using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/history")]
    public class AdminHistoryController : ControllerBase
    {
        private readonly IAdminHistoryService _adminHistoryService;
        public AdminHistoryController(IAdminHistoryService adminHistoryService)
        {
            _adminHistoryService = adminHistoryService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistoryDTO>>> GetAllHistory()
        {
            var histories = await _adminHistoryService.GetAllHistoryAsync();
            return Ok(histories);
        }

        [HttpGet("{userId}/adminProductHistoryByUserId")]
        public async Task<ActionResult<List<HistoryDTO>>> GetHistoryByUserId(int userId)
        {
            var historyList = await _adminHistoryService.GetHistoryByUserIdAsync(userId);

            if (historyList == null || historyList.Count == 0)
            {
                return NotFound($"No history found for user with ID {userId}.");
            }

            return Ok(historyList);
        }

        [HttpDelete("{historyId}/deleteProducthistory")]
        public async Task<IActionResult> DeleteHistory(int historyId)
        {
            var result = await _adminHistoryService.DeleteHistoryAsync(historyId);
            if (!result)
            {
                return NotFound($"History with ID {historyId} not found.");
            }

            return Ok($"History with ID {historyId} deleted successfully.");
        }

        [HttpDelete("clear/{userId}/clearHistory")]
        public async Task<IActionResult> ClearAllHistory(int userId)
        {
            var result = await _adminHistoryService.ClearHistoryAsync(userId);
            if (!result)
            {
                return NotFound("No history records found.");
            }

            return Ok("All history records deleted successfully.");
        }

    }
}
