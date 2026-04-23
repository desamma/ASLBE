using BussinessObjects.DTOs.Admin;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Services.IServices;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ServiceResult<List<AdminTransactionDto>>> GetAllTransactionsAsync(string? status = null, string? orderCode = null)
        {
            var query = _context.Transactions
                .Include(t => t.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status.ToLower() == status.ToLower());
            }

            if (!string.IsNullOrEmpty(orderCode))
            {
                query = query.Where(t => t.OrderCode.ToString().Contains(orderCode));
            }

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
                Message = "Successfully retrieved transactions."
            };
        }

        public async Task<ServiceResult<List<AdminShopPurchaseDto>>> GetAllShopPurchasesAsync(string? searchName = null, int? quantity = null)
        {
            var query = _context.ShopPurchases
                .Include(sp => sp.User)
                .Include(sp => sp.ShopItem)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(sp =>
                    (sp.User != null && sp.User.UserName.Contains(searchName)) ||
                    (sp.ShopItem != null && sp.ShopItem.Name.Contains(searchName)));
            }

            // ĐÃ XÓA ĐOẠN FILTER THEO TYPE Ở ĐÂY ĐỂ TRÁNH LỖI CS1061

            if (quantity.HasValue)
            {
                query = query.Where(sp => sp.Quantity == quantity.Value);
            }

            var purchases = await query.OrderByDescending(sp => sp.PurchaseDate)
                .Select(sp => new AdminShopPurchaseDto
                {
                    Id = sp.Id,
                    UserId = sp.UserId,
                    UserName = sp.User != null ? sp.User.UserName : "Unknown",
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
                Message = "Successfully retrieved shop purchase history."
            };
        }
    }
}