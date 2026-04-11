using BussinessObjects.DTOs.Payment;
using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Services.IServices;
using Services.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.IServices
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            ApplicationDbContext db,
            PayOSClient payOS,
            IConfiguration config,
            ILogger<PaymentService> logger)
        {
            _db = db;
            _payOS = payOS;
            _config = config;
            _logger = logger;
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1. Lấy danh sách gói VP
        // ────────────────────────────────────────────────────────────────────────
        public Task<List<VpPackageDto>> GetPackagesAsync()
        {
            var result = VpPackageCatalog.Packages.Values.Select(p => new VpPackageDto
            {
                VpKey = p.TotalVp,
                DisplayName = p.DisplayName,
                BaseVp = p.Vp,
                BonusVp = p.BonusVp,
                TotalVp = p.TotalVp,
                PriceVnd = p.PriceVnd
            }).OrderBy(p => p.PriceVnd).ToList();

            return Task.FromResult(result);
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2. Tạo payment link
        // ────────────────────────────────────────────────────────────────────────
        public async Task<CreatePaymentResponse> CreatePaymentAsync(Guid userId, BussinessObjects.DTOs.Payment.CreatePaymentRequest request)
        {
            // Validate gói VP
            if (!VpPackageCatalog.TryGet(request.VpPackage, out var package) || package is null)
                return Fail("Gói VP không hợp lệ.");

            // OrderCode phải là long dương, unique — dùng timestamp ms
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // URL sau khi thanh toán xong / huỷ — FE sẽ xử lý tiếp
            string baseUrl = _config["PayOS:ReturnBaseUrl"] ?? "http://localhost:3000";
            string returnUrl = $"{baseUrl}/payment/success?orderCode={orderCode}";
            string cancelUrl = $"{baseUrl}/payment/cancel?orderCode={orderCode}";

            // SDK v2: CreatePaymentLinkRequest từ PayOS.Models
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)package.PriceVnd,
                Description = $"Nap {package.TotalVp} VP",
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };

            try
            {
                // SDK v2: PaymentRequests.CreateAsync()
                CreatePaymentLinkResponse result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                // Lưu Transaction vào DB với Status = Pending
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderCode = orderCode,
                    Name = package.DisplayName,
                    Description = $"Nạp {package.TotalVp} VP vào tài khoản",
                    Type = "TopUp",
                    Amount = package.PriceVnd,
                    CurrencyAwarded = package.TotalVp,
                    Status = "Pending",
                    CheckoutUrl = result.CheckoutUrl,
                    QrCodeUrl = result.QrCode,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created payment link. OrderCode={OrderCode} UserId={UserId}", orderCode, userId);

                return new CreatePaymentResponse
                {
                    Success = true,
                    Message = "Tạo link thanh toán thành công",
                    CheckoutUrl = result.CheckoutUrl,
                    OrderCode = orderCode,
                    TransactionId = transaction.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS createPaymentLink failed. UserId={UserId}", userId);
                return Fail($"Lỗi khi tạo link thanh toán: {ex.Message}");
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3. Xử lý webhook từ PayOS
        // ────────────────────────────────────────────────────────────────────────
        public async Task<bool> HandleWebhookAsync(BussinessObjects.DTOs.Payment.PayOSWebhookPayload payload)
        {
            _logger.LogInformation("Webhook received. Code={Code} OrderCode={OrderCode}",
                payload.Code, payload.Data?.OrderCode);

            // SDK v2: class Webhook + WebhookData đều nằm trong PayOS.Models
            try
            {
                var webhookBody = new Webhook
                {
                    Code = payload.Code,
                    Description = payload.Desc,
                    Success = payload.Success,
                    Signature = payload.Signature,
                    Data = payload.Data is null ? null : new WebhookData
                    {
                        OrderCode = payload.Data.OrderCode,
                        Amount = payload.Data.Amount,
                        Description = payload.Data.Description,
                        AccountNumber = payload.Data.AccountNumber,
                        Reference = payload.Data.Reference,
                        TransactionDateTime = payload.Data.TransactionDateTime,
                        Currency = payload.Data.Currency,
                        PaymentLinkId = payload.Data.PaymentLinkId,
                        Code = payload.Data.Code,
                        Description2 = payload.Data.Desc,
                        CounterAccountBankId = payload.Data.CounterAccountBankId,
                        CounterAccountBankName = payload.Data.CounterAccountBankName,
                        CounterAccountName = payload.Data.CounterAccountName,
                        CounterAccountNumber = payload.Data.CounterAccountNumber,
                        VirtualAccountName = payload.Data.VirtualAccountName,
                        VirtualAccountNumber = payload.Data.VirtualAccountNumber
                    }
                };

                await _payOS.Webhooks.VerifyAsync(webhookBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Webhook signature invalid. OrderCode={OrderCode}. Error={Error}",
                    payload.Data?.OrderCode, ex.Message);
                return false;
            }

            if (payload.Data is null) return false;

            long orderCode = payload.Data.OrderCode;

            var transaction = await _db.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

            if (transaction is null)
            {
                _logger.LogWarning("Transaction not found. OrderCode={OrderCode}", orderCode);
                return false;
            }

            // Idempotent — bỏ qua nếu đã xử lý rồi
            if (transaction.Status == "Paid")
            {
                _logger.LogInformation("Transaction already paid. OrderCode={OrderCode}", orderCode);
                return true;
            }

            // PayOS trả code "00" = thành công
            if (payload.Code == "00" && payload.Success)
            {
                transaction.Status = "Paid";
                transaction.PaidAt = DateTime.UtcNow;

                // Cộng VP vào tài khoản user
                if (transaction.User is not null)
                {
                    transaction.User.CurrencyAmount += transaction.CurrencyAwarded;
                    _logger.LogInformation("Credited {VP} VP to UserId={UserId}", transaction.CurrencyAwarded, transaction.UserId);
                }
            }
            else
            {
                transaction.Status = "Failed";
            }

            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────
        // 4. Lịch sử giao dịch của user
        // ────────────────────────────────────────────────────────────────────────
        public async Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId)
        {
            return await _db.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => ToDto(t))
                .ToListAsync();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 5. Kiểm tra đơn theo orderCode (dùng sau khi return từ PayOS)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<TransactionDto?> GetTransactionByOrderCodeAsync(long orderCode)
        {
            var t = await _db.Transactions.FirstOrDefaultAsync(x => x.OrderCode == orderCode);
            return t is null ? null : ToDto(t);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────

        private static TransactionDto ToDto(Transaction t) => new()
        {
            Id = t.Id,
            OrderCode = t.OrderCode,
            Name = t.Name,
            Amount = t.Amount,
            CurrencyAwarded = t.CurrencyAwarded,
            Status = t.Status,
            CheckoutUrl = t.CheckoutUrl,
            CreatedAt = t.CreatedAt,
            PaidAt = t.PaidAt
        };

        private static CreatePaymentResponse Fail(string msg) => new() { Success = false, Message = msg };
    }
}