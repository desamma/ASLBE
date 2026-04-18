using BussinessObjects.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAdminPaymentService
    {
        // Lấy lịch sử nạp tiền (có thể lọc theo trạng thái như "Paid", "Pending")
        Task<ServiceResult<List<AdminTransactionDto>>> GetAllTransactionsAsync(string? status = null);

        // Lấy lịch sử mua đồ trong Shop
        Task<ServiceResult<List<AdminShopPurchaseDto>>> GetAllShopPurchasesAsync();
    }
}
