using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Models;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class SalesService : ISalesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public SalesService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<SalesDTO>> GetAllSalesAsync()
        {
            var allSalesData = await _context.Sales
       .Include(sale => sale.Order)
       .ThenInclude(order => order.OrderDetails)
       .ThenInclude(orderDetail => orderDetail.Product)
       .ToListAsync();

            var groupedSalesData = allSalesData
                .GroupBy(sale => sale.SaleDate.Date)
                .Select(group => new SalesDTO
                {
                    SaleDate = group.Key,
                    TotalAmount = group.Sum(sale => sale.Order.OrderDetails.Sum(orderDetail => orderDetail.Product.Price * orderDetail.Quantity)),
                    CostPrice = group.Sum(sale => sale.Order.OrderDetails.Sum(orderDetail => orderDetail.Product.CostPrice * orderDetail.Quantity)),
                    SellingPrice = group.Sum(sale => sale.Order.OrderDetails.Sum(orderDetail => orderDetail.Product.SellingPrice * orderDetail.Quantity)),
                    TotalProfit = group.Sum(sale => sale.Order.OrderDetails.Sum(orderDetail => (orderDetail.Product.SellingPrice - orderDetail.Product.CostPrice) * orderDetail.Quantity)),
                    TotalProductsSold = group.Sum(sale => sale.Order.OrderDetails.Sum(orderDetail => orderDetail.Quantity))
                })
                .ToList();

            return groupedSalesData;

        }


        public async Task<SalesDTO> AddSaleAsync(CreateSaleDTO createSaleDTO)
        {
            var existingSale = await _context.Sales
                                    .Where(s => s.OrderId == createSaleDTO.OrderId)
                                    .Where(s => (s.StartDate <= createSaleDTO.EndDate && s.EndDate >= createSaleDTO.StartDate))
                                    .FirstOrDefaultAsync();

            if (existingSale != null)
            {
                throw new Exception("A sale for this order already exists in the specified date range.");
            }

            var order = await _context.Orders
                                .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .FirstOrDefaultAsync(o => o.OrderId == createSaleDTO.OrderId);

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            var sale = new Sale
            {
                OrderId = createSaleDTO.OrderId,
                UserId = createSaleDTO.UserId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(3),
                SaleDate = DateTime.Now,
                TotalAmount = createSaleDTO.TotalAmount
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return _mapper.Map<SalesDTO>(sale);

        }


        public async Task<IEnumerable<SalesDTO>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .Include(s => s.User)
                .Include(s => s.Order)
                .ThenInclude(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SalesDTO>>(sales);
        }

        public async Task<SalesDTO> GetSaleByOrderIdAsync(int orderId)
        {
            var sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.Order)
                .ThenInclude(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(s => s.OrderId == orderId);

            if (sale == null)
                throw new Exception("Sale not found.");

            return _mapper.Map<SalesDTO>(sale);
        }
        public async Task<SalesComparisonResultDTO> CompareSalesAsync(SalesComparisonDTO currentPeriod, SalesComparisonDTO previousPeriod)
        {
            var currentPeriodSales = await _context.Sales
                .Where(s => s.SaleDate >= currentPeriod.StartDate && s.SaleDate <= currentPeriod.EndDate)
                .ToListAsync();

            var previousPeriodSales = await _context.Sales
                .Where(s => s.SaleDate >= previousPeriod.StartDate && s.SaleDate <= previousPeriod.EndDate)
                .ToListAsync();

            var result = new SalesComparisonResultDTO
            {
                TotalSalesThisPeriod = currentPeriodSales.Count(),
                TotalSalesPreviousPeriod = previousPeriodSales.Count(),
                RevenueThisPeriod = currentPeriodSales.Sum(s => s.TotalAmount),
                RevenuePreviousPeriod = previousPeriodSales.Sum(s => s.TotalAmount)
            };

            return result;
        }
    }
}










