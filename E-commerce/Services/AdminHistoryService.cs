using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class AdminHistoryService:IAdminHistoryService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public AdminHistoryService(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HistoryDTO>> GetAllHistoryAsync()
        {
            var histories = await _context.Histories
                .Include(h => h.User)
                .Include(h => h.Product)
                .Select(h => new HistoryDTO
                {
                    HistoryId = h.HistoryId,
                    ActionType = h.ActionType,
                    Details = h.Details,
                    ActionDate = h.ActionDate,
                    UserId = h.UserId,
                    UserName = h.User != null ? h.User.UserName : null,
                    ProductId = h.ProductId,
                    ProductName = h.Product != null ? h.Product.ProductName : null,
                    IsAdminAction = h.IsAdminAction
                })
                .ToListAsync();

            return histories;
        }

        public async Task<List<HistoryDTO>> GetHistoryByUserIdAsync(int userId)
        {
            var histories = await _context.Histories
                .Include(h => h.Product)
                .Include(u=>u.User)
                .Where(h => h.UserId == userId)
                .ToListAsync();

            return histories.Select(h => new HistoryDTO
            {
                HistoryId = h.HistoryId,
                UserId = h.UserId,
                UserName = h.User?.UserName,
                ActionType = h.ActionType,
                Details = h.Details,
                ActionDate = h.ActionDate,
                ProductName = h.Product?.ProductName, 
                ProductImage = h.Product?.Image ,
                Price = h.Product?.Price,
                ProductId = h.Product?.ProductId
            }).ToList();
        }


        public async Task<bool> DeleteHistoryAsync(int historyId)
        {
            var history = await _context.Histories.FindAsync(historyId);

            if (history == null)
            {
                return false;
            }

            _context.Histories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearHistoryAsync(int userId)
        {
            var allHistoryRecords = await _context.Histories
                .Where(i=>i.UserId==userId)
                .ToListAsync();

            if (allHistoryRecords.Count == 0)
            {
                return false; 
            }

            _context.Histories.RemoveRange(allHistoryRecords);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
