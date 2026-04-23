using BussinessObjects.DTOs.Admin;

namespace Services.IServices
{
    public interface IAdminPaymentService
    {
        Task<ServiceResult<List<AdminTransactionDto>>> GetAllTransactionsAsync(string? status = null);

        Task<ServiceResult<List<AdminShopPurchaseDto>>> GetAllShopPurchasesAsync();
    }
}
