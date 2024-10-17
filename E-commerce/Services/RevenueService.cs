using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly DataContext _context;

        public RevenueService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<RevenueDTO>> CalculateTotalRevenueAsync()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating total revenue: {ex.Message}");
                return new List<RevenueDTO>();        
            }
        }

        public async Task<RevenueDTO> GetRevenueByDateAsync(DateTime date)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving revenue by date: {ex.Message}");
                return new RevenueDTO { Date = date, TotalRevenue = 0 };     
            }
        }

        public async Task<RevenueByDateDTO> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving revenue by date range: {ex.Message}");
                return new RevenueByDateDTO { StartDate = startDate, EndDate = endDate, TotalRevenue = 0 };     
            }
        }
    }
}
