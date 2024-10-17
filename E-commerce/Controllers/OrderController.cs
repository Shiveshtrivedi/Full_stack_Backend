using AutoMapper;
using E_commerce.DTOs;
using E_commerce.Services;
using E_commerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace E_commerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderServices _orderServices;

        public OrderController(IOrderServices orderServices)
        {
            _orderServices = orderServices;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderServices.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}/getOrderByOrderId")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderServices.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound(new { message = "No order" });
            }

            return Ok(order);
        }

        [HttpGet("user/{userId}/getOrderByUserId")]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            var orders = await _orderServices.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpPost("placeOrder")]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDTO orderDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderResult = await _orderServices.PlaceOrderAsync(orderDTO);

            if (orderResult == null)
            {
                return BadRequest(new { message = "Unable to place the order" });
            }

            return Ok(orderResult);
        }


        [HttpPut("{orderId}/updateStatus")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] OrderUpdateDTO orderUpdateDTO)
        {
            var updatedOrder = await _orderServices.UpdateOrderAsync(orderId, orderUpdateDTO);

            if (updatedOrder == null)
            {
                return BadRequest(new { message = "Failed to update the order" });
            }

            return Ok(updatedOrder);
        }
    }
}
