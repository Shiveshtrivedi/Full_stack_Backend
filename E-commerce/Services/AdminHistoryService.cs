using AutoMapper;
using E_commerce.Context;
using E_commerce.DTOs;
using E_commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services
{
    public class AdminHistoryService : IAdminHistoryService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public AdminHistoryService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HistoryDTO>> GetAllHistoryAsync()
        {
            try
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
            catch(Exception ex)
            {
                return Enumerable.Empty<HistoryDTO>();
            }

        }

        public async Task<List<HistoryDTO>> GetHistoryByUserIdAsync(int userId)
        {
            try
            {
                var histories = await _context.Histories
                    .Include(h => h.Product)
                    .Include(u => u.User)
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
                    ProductImage = h.Product?.Image,
                    Price = h.Product?.Price,
                    ProductId = h.Product?.ProductId
                }).ToList();
            }
            catch(Exception ex)
            {
                return new List<HistoryDTO>();
            }
        }


        public async Task<bool> DeleteHistoryAsync(int historyId)
        {
            try
            {
                var history = await _context.Histories.FindAsync(historyId);

                if (history == null)
                {
                    return false;
                }

                history.DeleteFlag = true;
                await _context.SaveChangesAsync();

                var product = await _context.Products.FindAsync(history.ProductId);
                if (product != null && product.DeleteFlag)
                {
                    await DeleteProductAndHistory(historyId, product.ProductId);
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ClearHistoryAsync(int userId)
        {
            try
            {
                var allHistoryRecords = await _context.Histories
                    .Where(i => i.UserId == userId)
                    .ToListAsync();

                if (allHistoryRecords.Count == 0)
                {
                    return false;
                }

                _context.Histories.RemoveRange(allHistoryRecords);
                await _context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private async Task DeleteProductAndHistory(int historyId, int productId)
        {
            try
            {
                var history = await _context.Histories.FindAsync(historyId);
                if (history != null)
                {
                    _context.Histories.Remove(history);
                }

                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
                    if (inventory != null)
                    {
                        _context.Inventories.Remove(inventory);
                    }

                    _context.Products.Remove(product);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("error message" + ex.Message);
            }
        }

    }
}
