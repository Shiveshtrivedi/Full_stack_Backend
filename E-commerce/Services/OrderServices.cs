using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace E_commerce.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly RazorpayService _razorpayService;
        public OrderServices(DataContext context, IMapper mapper,RazorpayService razorpayService)
        {
            _context = context;
            _mapper = mapper;
            _razorpayService = razorpayService;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)       
                .Include(o => o.ShippingAddress)      
                .Include(o => o.OrderDetails)    
                    .ThenInclude(od => od.Product)       
                .ToListAsync(); 

            return _mapper.Map<IEnumerable<Order>, IEnumerable<OrderDTO>>(orders);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<Order>, IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails)
                                             .ThenInclude(p => p.Product)
                                             .Include(s => s.ShippingAddress)
                                             .Where(o => o.OrderId == orderId)
                                             .FirstOrDefaultAsync();

            if (order == null)
                return null;

            var orderDto = new OrderDTO
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                PaymentMethod = order.PaymentMethod,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDTO
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList()
            };
            return orderDto;
        }

        public async Task<IEnumerable<OrderDTO>> UpdateOrderAsync(int orderId, OrderUpdateDTO orderUpdateDTO)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(orderUpdateDTO.Status))
            {
                if (Enum.TryParse(typeof(OrderStatus), orderUpdateDTO.Status, out var status))
                {
                    order.Status = (OrderStatus)status;
                }
                else
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(orderUpdateDTO.PaymentMethod))
            {
                order.PaymentMethod = orderUpdateDTO.PaymentMethod;
            }

            if (!string.IsNullOrEmpty(orderUpdateDTO.TransctionId))
            {
                order.TransctionId = orderUpdateDTO.TransctionId;
            }

            //await _context.SaveChangesAsync();

            foreach(var orderDetail in order.OrderDetails)
            {
                var product = orderDetail.Product;

                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.ProductId);
                if (inventory != null)
                {
                    inventory.StockAvailable -= orderDetail.Quantity;
                }

                var sale = new Sale
                {
                    OrderId = orderId,
                    UserId = order.UserId,      
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    SaleDate = DateTime.Now,
                    TotalAmount = orderDetail.Quantity * orderDetail.Price      
                };

                _context.Sales.Add(sale);
            }

            await _context.SaveChangesAsync();

            var updatedOrder = await _context.Orders
               .Include(o => o.OrderDetails)
                   .ThenInclude(od => od.Product)
               .Include(o => o.ShippingAddress)
               .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (updatedOrder == null)
            {
                return null;
            }


            return new List<OrderDTO> { _mapper.Map<Order, OrderDTO>(updatedOrder) };
        }




        public async Task<OrderDTO> PlaceOrderAsync(CreateOrderDTO orderDTO)
        {
            var user = await _context.Users.FindAsync(orderDTO.UserId);
            if (user == null)
            {
                return null;    
            }

            var order = new Order
            {
                UserId = orderDTO.UserId,
                PaymentMethod = orderDTO.PaymentMethod,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>(),

            };

            decimal totalAmount = 0;    

            foreach (var item in orderDTO.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;        

                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Order = order
                };

                order.OrderDetails.Add(orderDetail);
                totalAmount += item.Price * item.Quantity;    
            }
            var apiKey = Environment.GetEnvironmentVariable("RAZORPAY_KEY");

            var razorpayOrder = await _razorpayService.CreateOrderAsync(totalAmount, "INR", order.OrderId.ToString());


            if (razorpayOrder == null)
            {
                return null;         
            }

            order.RazorpayOrderId = razorpayOrder["id"].ToString();
            order.TransctionId = razorpayOrder["transactionId"]?.ToString() ?? ""; 


            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderDTOResult = _mapper.Map<Order, OrderDTO>(order);

            return orderDTOResult;
        }

    }
}
