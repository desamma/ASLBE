using BussinessObjects.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IPaymentService
    {
        /// <summary>Lấy toàn bộ gói VP để hiển thị trên UI</summary>
        Task<List<VpPackageDto>> GetPackagesAsync();

        /// <summary>Tạo payment link PayOS, lưu Transaction Pending</summary>
        Task<CreatePaymentResponse> CreatePaymentAsync(Guid userId, CreatePaymentRequest request);

        /// <summary>Xử lý webhook PayOS gửi về sau khi thanh toán</summary>
        Task<bool> HandleWebhookAsync(PayOSWebhookPayload payload);

        /// <summary>Lấy lịch sử giao dịch của user</summary>
        Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId);

        /// <summary>Kiểm tra trạng thái 1 đơn (dùng khi return từ PayOS)</summary>
        Task<TransactionDto?> GetTransactionByOrderCodeAsync(long orderCode);
        Task<decimal> GetUserBalanceAsync(Guid userId);
    }
}
