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
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public SalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDTO>>> GetAllSales()
        {
            var sales = await _salesService.GetAllSalesAsync();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesDTO>> GetSaleByOrderId(int id)
        {
            try
            {
                var sale = await _salesService.GetSaleByOrderIdAsync(id);
                return Ok(sale);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult<SalesDTO>> AddSale(CreateSaleDTO createSaleDTO)
        {
            var sale = await _salesService.AddSaleAsync(createSaleDTO);
            return CreatedAtAction(nameof(GetSaleByOrderId), new { id = sale.SaleId }, sale);
        }

        [HttpGet("daterange")]
        public async Task<ActionResult<IEnumerable<SalesDTO>>> GetSalesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var sales = await _salesService.GetSalesByDateRangeAsync(startDate, endDate);
            return Ok(sales);
        }

        [HttpPost("/saleComparsion")]
        public async Task<ActionResult<SalesComparisonResultDTO>> CompareSales([FromBody] SalesComparisonRequestDTO comparisonRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _salesService.CompareSalesAsync(comparisonRequest.CurrentPeriod, comparisonRequest.PreviousPeriod);

                if (result == null)
                {
                    return NotFound("No sales data found for the given periods.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
