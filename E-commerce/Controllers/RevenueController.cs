using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Authorize]
    [Route("api/revenue")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _revenueService.CalculateTotalRevenueAsync();
            return Ok(totalRevenue);
        }

        [HttpGet("{date}")]
        public async Task<IActionResult> GetRevenueByDate(DateTime date)
        {
            var revenue = await _revenueService.GetRevenueByDateAsync(date);
            return Ok(revenue);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRevenueByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var revenue = await _revenueService.GetRevenueByDateRangeAsync(startDate, endDate);
            return Ok(revenue);
        }
    }
}
