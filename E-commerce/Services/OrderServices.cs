using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace E_commerce.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly RazorpayService _razorpayService;
        private readonly MQTTService _mqttService;
        public OrderServices(DataContext context, IMapper mapper,RazorpayService razorpayService, MQTTService mqttService)
        {
            _context = context;
            _mapper = mapper;
            _razorpayService = razorpayService;
            _mqttService = mqttService;
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
                return null; // Handle case when order does not exist
            }

            // Update order details only if necessary
            if (!string.IsNullOrEmpty(orderUpdateDTO.Status))
            {
                if (Enum.TryParse(typeof(OrderStatus), orderUpdateDTO.Status, out var status))
                {
                    order.Status = (OrderStatus)status;
                }
                else
                {
                    return null; // Handle invalid status case
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

            // List to hold product updates for publishing later
            var updatedProducts = new List<ProductSaleDTO>();

            var user = await _context.Users.FindAsync(order.UserId);
            var userName = user?.UserName ?? string.Empty;

            foreach (var orderDetail in order.OrderDetails)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == orderDetail.ProductId);

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == orderDetail.ProductId);

                if (inventory == null)
                {
                    throw new Exception($"Inventory not found for Product ID {orderDetail.ProductId}.");
                }

                // Check if enough stock is available for the order
                if (inventory.StockAvailable < orderDetail.Quantity)
                {
                    throw new Exception($"Insufficient stock available for Product ID {orderDetail.ProductId}.");
                }

                // Update product stock
                if (product != null)
                {
                    product.Stock -= orderDetail.Quantity; // Decrease product stock
                }

                // Update inventory stock
                inventory.StockSold += orderDetail.Quantity;
                inventory.StockAvailable -= orderDetail.Quantity;

                var sale = new Sale
                {
                    OrderId = orderId,
                    UserId = order.UserId, // Assuming UserId is part of the Order entity
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    SaleDate = DateTime.Now,
                    TotalAmount = orderDetail.Quantity * orderDetail.Price // Assuming Price is part of OrderDetail
                    
                };

                _context.Sales.Add(sale);

                // Publish sales update
                var salesPayload = new
                {
                    SaleId = sale.SalesId, // Assuming SalesId is the key in the Sale model
                    orderId = sale.OrderId,
                    userId = sale.UserId,
                    userName = userName,
                    saleDate = sale.SaleDate,
                    startDate = sale.StartDate,
                    endDate = sale.EndDate,
                    totalAmount = sale.TotalAmount, 
                    productName = product?.ProductName
                };
                await _mqttService.PublishAsync("sales-updates", JsonConvert.SerializeObject(salesPayload));




                updatedProducts.Add(new ProductSaleDTO
                {
                    ProductId = orderDetail.ProductId,
                    QuantitySold = orderDetail.Quantity
                });

                // Publish MQTT message with updated stock information for both product and inventory
                var stockUpdateMessage = new
                {
                    ProductId = product.ProductId,
                    StockAvailable = inventory.StockAvailable,
                    StockSold = inventory.StockSold,
                    ProductStock = product.Stock // include the product stock in the message
                };

                var jsonMessage = JsonConvert.SerializeObject(stockUpdateMessage);
                await _mqttService.PublishAsync("inventory-updates", jsonMessage); // Publish inventory updates
            }

            // Save changes to both the inventory and product tables
            await _context.SaveChangesAsync();

            // Publish order update message for real-time notification
            var orderMessage = new
            {
                OrderId = order.OrderId,
                Status = order.Status.ToString(),
                PaymentMethod = order.PaymentMethod
            };

            var orderJsonMessage = JsonConvert.SerializeObject(orderMessage);
            await _mqttService.PublishAsync("order/updates", orderJsonMessage); // Publish order updates

            // Return updated orders as an IEnumerable<OrderDTO>
            return new List<OrderDTO> { _mapper.Map<Order, OrderDTO>(order) };
        }




        //public async Task<IEnumerable<OrderDTO>> UpdateOrderAsync(int orderId, OrderUpdateDTO orderUpdateDTO)
        //{
        //    var order = await _context.Orders
        //        .Include(o => o.OrderDetails)
        //            .ThenInclude(od => od.Product)
        //        .Include(o => o.ShippingAddress)
        //        .FirstOrDefaultAsync(o => o.OrderId == orderId);

        //    if (order == null)
        //    {
        //        return null;
        //    }

        //    if (!string.IsNullOrEmpty(orderUpdateDTO.Status))
        //    {
        //        if (Enum.TryParse(typeof(OrderStatus), orderUpdateDTO.Status, out var status))
        //        {
        //            order.Status = (OrderStatus)status;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(orderUpdateDTO.PaymentMethod))
        //    {
        //        order.PaymentMethod = orderUpdateDTO.PaymentMethod;
        //    }

        //    if (!string.IsNullOrEmpty(orderUpdateDTO.TransctionId))
        //    {
        //        order.TransctionId = orderUpdateDTO.TransctionId;
        //    }

        //    //await _context.SaveChangesAsync();

        //    foreach(var orderDetail in order.OrderDetails)
        //    {
        //        var product = orderDetail.Product;

        //        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.ProductId);
        //        if (inventory != null)
        //        {
        //            inventory.StockAvailable -= orderDetail.Quantity;

        //            var inventoryPayload = new
        //            {
        //                ProductId = product.ProductId,
        //                StockAvailable = inventory.StockAvailable
        //            };
        //            await _mqttService.PublishAsync("inventory-updates", JsonConvert.SerializeObject(inventoryPayload));

        //        }

        //        var sale = new Sale
        //        {
        //            OrderId = orderId,
        //            UserId = order.UserId,      
        //            StartDate = DateTime.Now,
        //            EndDate = DateTime.Now.AddDays(3),
        //            SaleDate = DateTime.Now,
        //            TotalAmount = orderDetail.Quantity * orderDetail.Price      
        //        };

        //        _context.Sales.Add(sale);

        //        var salesPayload = new
        //        {
        //            SaleId = sale.SalesId,
        //            OrderId = sale.OrderId,
        //            TotalAmount = sale.TotalAmount,
        //            SaleDate = sale.SaleDate
        //        };
        //        await _mqttService.PublishAsync("sales-updates", JsonConvert.SerializeObject(salesPayload));

        //    }

        //    await _context.SaveChangesAsync();

        //    var updatedOrder = await _context.Orders
        //       .Include(o => o.OrderDetails)
        //           .ThenInclude(od => od.Product)
        //       .Include(o => o.ShippingAddress)
        //       .FirstOrDefaultAsync(o => o.OrderId == orderId);

        //    if (updatedOrder == null)
        //    {
        //        return null;
        //    }


        //    return new List<OrderDTO> { _mapper.Map<Order, OrderDTO>(updatedOrder) };
        //}




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
