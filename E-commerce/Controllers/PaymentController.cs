using Microsoft.AspNetCore.Mvc;
using E_commerce.Services;
using Razorpay.Api;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IRazorpayService _razorpayService;

        public PaymentController(IRazorpayService razorpayService)
        {
            _razorpayService = razorpayService;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _razorpayService.CreateOrderAsync(request.Amount, request.Currency, request.Receipt);

                if (order == null)
                {
                    return BadRequest(new { message = "Order creation failed." });
                }

                return Ok(new
                {
                    orderId = order["id"].ToString(),
                    amount = order["amount"],
                    currency = order["currency"]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
    public class CreateOrderRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; } = "order_rcptid_11";
    }

}
