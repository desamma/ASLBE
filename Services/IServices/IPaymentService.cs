using BussinessObjects.DTOs.Payment;

namespace Services.IServices
{
    public interface IPaymentService
    {
        Task<List<VpPackageDto>> GetPackagesAsync();

        Task<CreatePaymentResponse> CreatePaymentAsync(Guid userId, CreatePaymentRequest request);

        Task<bool> HandleWebhookAsync(PayOSWebhookPayload payload);

        Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId);

        Task<TransactionDto?> GetTransactionByOrderCodeAsync(long orderCode);
        Task<decimal> GetUserBalanceAsync(Guid userId);
    }
}
