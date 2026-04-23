using BussinessObjects.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAdminPaymentService
    {
        Task<ServiceResult<List<AdminTransactionDto>>> GetAllTransactionsAsync(string? status = null, string? orderCode = null);

        Task<ServiceResult<List<AdminShopPurchaseDto>>> GetAllShopPurchasesAsync(string? searchName = null, int? quantity = null);
    }
}