using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class RevenueService:IRevenueService
    {
        private readonly DataContext _context;

        public RevenueService(DataContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateTotalRevenueAsync()
        {
            return await _context.Sales.SumAsync(s=>s.TotalAmount);
        }

        public async Task<RevenueDTO> GetRevenueByDateAsync(DateTime date)
        {
            var totalRevenue = await _context.Sales
                .Where(s => s.SaleDate.Date == date.Date)
                .SumAsync(s => s.TotalAmount);

            return new RevenueDTO
            {
                Date = date,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<RevenueByDateDTO> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var totalRevenue = await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .SumAsync(s => s.TotalAmount);

            return new RevenueByDateDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue
            };
        }
    }
}
