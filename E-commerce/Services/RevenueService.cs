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

        public async Task<List<RevenueDTO>> CalculateTotalRevenueAsync()
        {
            var revenueData = await _context.Sales
         .GroupBy(s => s.SaleDate.Date)       
         .Select(g => new RevenueDTO
         {
             Date = g.Key,     
             TotalRevenue = g.Sum(s => s.TotalAmount)       
         })
         .ToListAsync();

            return revenueData;
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
