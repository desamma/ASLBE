using BussinessObjects.DTOs.Admin;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class AdminPaymentService : IAdminPaymentService
    {
        private readonly ApplicationDbContext _context;

        public AdminPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<List<AdminTransactionDto>>> GetAllTransactionsAsync(string? status = null)
        {
            var query = _context.Transactions
                .Include(t => t.User)
                .AsNoTracking();

            // Nếu truyền status (vd: "Paid") thì lọc theo status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status.ToLower() == status.ToLower());
            }

            // Sắp xếp giao dịch mới nhất lên đầu
            var transactions = await query.OrderByDescending(t => t.CreatedAt)
                .Select(t => new AdminTransactionDto
                {
                    Id = t.Id,
                    OrderCode = t.OrderCode,
                    UserId = t.UserId,
                    UserName = t.User != null ? t.User.UserName : "Unknown",
                    Name = t.Name,
                    Type = t.Type,
                    Amount = t.Amount,
                    CurrencyAwarded = t.CurrencyAwarded,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    PaidAt = t.PaidAt
                }).ToListAsync();

            return new ServiceResult<List<AdminTransactionDto>>
            {
                Success = true,
                Data = transactions,
                Message = "Lấy danh sách giao dịch thành công"
            };
        }

        public async Task<ServiceResult<List<AdminShopPurchaseDto>>> GetAllShopPurchasesAsync()
        {
            var purchases = await _context.ShopPurchases
                .Include(sp => sp.User)
                .Include(sp => sp.ShopItem)
                .AsNoTracking()
                .OrderByDescending(sp => sp.PurchaseDate)
                .Select(sp => new AdminShopPurchaseDto
                {
                    Id = sp.Id,
                    UserId = sp.UserId,
                    UserName = sp.User != null ? sp.User.UserName : "Unknown",
                    // Giả định ShopItem có trường Name. Nếu là ItemName thì đổi thành sp.ShopItem.ItemName
                    ShopItemName = sp.ShopItem != null ? sp.ShopItem.Name : "Unknown Item",
                    Quantity = sp.Quantity,
                    PaymentType = sp.PaymentType,
                    AmountPaid = sp.AmountPaid,
                    PurchaseDate = sp.PurchaseDate
                }).ToListAsync();

            return new ServiceResult<List<AdminShopPurchaseDto>>
            {
                Success = true,
                Data = purchases,
                Message = "Lấy lịch sử mua hàng trong Shop thành công"
            };
        }
    }
}
