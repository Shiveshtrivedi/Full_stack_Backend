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
        public OrderServices(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User) // Include related User data if needed
                .Include(o => o.ShippingAddress) // Include Shipping Address if needed
                .Include(o => o.OrderDetails) // Include Order Details
                    .ThenInclude(od => od.Product) // Include Product details in Order Details
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
                //ShippingAddress = order.ShippingAddress != null ? new ShippingAddressDTO
                //{
                //    AddressLine = order.ShippingAddress.AddressLine1 ?? string.Empty,
                //    City = order.ShippingAddress.City ?? string.Empty,
                //    State = order.ShippingAddress.State ?? string.Empty,
                //    ZipCode = order.ShippingAddress.ZipCode 
                //} : null,
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
            // Find the order by its ID
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product) // Include related Product data
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return null; // Order not found
            }

            // Update the status if provided in the DTO
            if (!string.IsNullOrEmpty(orderUpdateDTO.Status))
            {
                if (Enum.TryParse(typeof(OrderStatus), orderUpdateDTO.Status, out var status))
                {
                    order.Status = (OrderStatus)status;
                }
                else
                {
                    return null; // Invalid status
                }
            }

            // Update the payment method if provided in the DTO
            if (!string.IsNullOrEmpty(orderUpdateDTO.PaymentMethod))
            {
                order.PaymentMethod = orderUpdateDTO.PaymentMethod;
            }

            // Save changes to the database
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Return all orders that include the updated order, mapped to OrderDTOs
            var updatedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.ShippingAddress)
                .Where(o => o.OrderId == orderId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<Order>, IEnumerable<OrderDTO>>(updatedOrders);
        }



        public async Task<int?> PlaceOrderAsync(CreateOrderDTO orderDTO)
        {
            var user = await _context.Users.FindAsync(orderDTO.UserId);
            //var shippingAddress = await _context.ShippingAddresses.FindAsync(orderDTO.ShippingAddressId);
            //|| shippingAddress == null

            if (user == null )
            {
                return null;
            }

            var order = new Order
            {
                UserId = orderDTO.UserId,
                //ShippingAddressID = orderDTO.ShippingAddressId,
                PaymentMethod = orderDTO.PaymentMethod,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderDetails = new List<OrderDetail>()
            };

            Console.WriteLine($" order placed are {order.UserId} ,{order.OrderDetails}");

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
                Console.WriteLine($"order detail are {orderDetail}");

                order.OrderDetails.Add(orderDetail);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order.OrderId;
        }


    }
}
