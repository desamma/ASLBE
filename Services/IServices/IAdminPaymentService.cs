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
        Task<ServiceResult<List<AdminTransactionDto>>> GetAllTransactionsAsync(string? status = null);

        Task<ServiceResult<List<AdminShopPurchaseDto>>> GetAllShopPurchasesAsync();
    }
}
