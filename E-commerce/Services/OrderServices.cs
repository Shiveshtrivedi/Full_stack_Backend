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
        public OrderServices(DataContext context, IMapper mapper, RazorpayService razorpayService, MQTTService mqttService)
        {
            _context = context;
            _mapper = mapper;
            _razorpayService = razorpayService;
            _mqttService = mqttService;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Order>, IEnumerable<OrderDTO>>(orders);
            }
            catch(Exception ex)
            {
                throw new Exception("An error occurred while retrieving orders.", ex);
            }
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId)
        {
            try
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
            catch(Exception ex)
            {
                throw new Exception("An error occurred while retrieving orders for the user.", ex);
            }
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int orderId)
        {
            try
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
            catch(Exception ex)
            {
                throw new Exception($"An error occurred while retrieving order with ID {orderId}.", ex);
            }
        }

        public async Task<IEnumerable<OrderDTO>> UpdateOrderAsync(int orderId, OrderUpdateDTO orderUpdateDTO)
        {
            try
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

                var updatedProducts = new List<ProductSaleDTO>();
                var totalQuantitySold = 0;

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

                    if (inventory.StockAvailable < orderDetail.Quantity)
                    {
                        throw new Exception($"Insufficient stock available for Product ID {orderDetail.ProductId}.");
                    }

                    if (product != null)
                    {
                        product.Stock -= orderDetail.Quantity;
                    }

                    inventory.StockSold += orderDetail.Quantity;
                    inventory.StockAvailable -= orderDetail.Quantity;

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

                    totalQuantitySold += orderDetail.Quantity;

                    var salesPayload = new
                    {
                        SaleId = sale.SalesId,
                        orderId = sale.OrderId,
                        userId = 0,
                        userName = userName,
                        saleDate = sale.SaleDate,
                        startDate = sale.StartDate,
                        endDate = sale.EndDate,
                        totalAmount = sale.TotalAmount,
                        productName = product?.ProductName,
                        totalProductsSold = totalQuantitySold
                    };
                    await _mqttService.PublishAsync("sales-updates", JsonConvert.SerializeObject(salesPayload));

                    var revenuePayload = new
                    {
                      
                        saleDate = sale.SaleDate,
                        totalAmount = sale.TotalAmount,
                    };
                    await _mqttService.PublishAsync("revenue-updates", JsonConvert.SerializeObject(salesPayload));





                    updatedProducts.Add(new ProductSaleDTO
                    {
                        ProductId = orderDetail.ProductId,
                        QuantitySold = orderDetail.Quantity
                    });

                    var stockUpdateMessage = new
                    {
                        ProductId = product.ProductId,
                        StockAvailable = inventory.StockAvailable,
                        StockSold = inventory.StockSold,
                        ProductStock = product.Stock
                    };

                    var jsonMessage = JsonConvert.SerializeObject(stockUpdateMessage);
                    await _mqttService.PublishAsync("inventory-updates", jsonMessage);
                }

                await _context.SaveChangesAsync();

                var orderMessage = new
                {
                    OrderId = order.OrderId,
                    Status = order.Status.ToString(),
                    PaymentMethod = order.PaymentMethod
                };

                var orderJsonMessage = JsonConvert.SerializeObject(orderMessage);
                await _mqttService.PublishAsync("order/updates", orderJsonMessage);

                return new List<OrderDTO> { _mapper.Map<Order, OrderDTO>(order) };
            }
            catch(Exception ex)
            {
                throw new Exception($"An error occurred while updating the order with ID {orderId}.", ex);
            }
        }

        public async Task<OrderDTO> PlaceOrderAsync(CreateOrderDTO orderDTO)
        {
            try
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
            catch(Exception ex)
            {
                throw new Exception("An error occurred while placing the order.", ex);
            }
        }

    }
}
