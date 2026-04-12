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

        public async Task<CreatePaymentResponse> CreatePaymentAsync(Guid userId, BussinessObjects.DTOs.Payment.CreatePaymentRequest request)
        {
            _logger.LogInformation("CreatePayment START - UserId={UserId}, VpPackage={VpPackage}", userId, request.VpPackage);

            if (!VpPackageCatalog.TryGet(request.VpPackage, out var package) || package is null)
            {
                _logger.LogError("Invalid VP package: {VpPackage}", request.VpPackage);
                return Fail("Goi VP khong hop le.");
            }

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _logger.LogInformation("Generated OrderCode={OrderCode}", orderCode);

            string baseUrl = _config["PayOS:ReturnBaseUrl"] ?? "https://localhost:7032/Payment/Result";
            string returnUrl = $"{baseUrl}?orderCode={orderCode}";
            string cancelUrl = $"{baseUrl}?orderCode={orderCode}&cancelled=true";

            _logger.LogInformation("ReturnUrl={ReturnUrl}", returnUrl);

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
                _logger.LogInformation("Calling PayOS.PaymentRequests.CreateAsync()");
                CreatePaymentLinkResponse result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);
                _logger.LogInformation("PayOS response received. CheckoutUrl={CheckoutUrl}", result.CheckoutUrl);

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderCode = orderCode,
                    Name = package.DisplayName,
                    Description = $"Nap {package.TotalVp} VP vao tai khoan",
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
                _logger.LogInformation("Transaction saved. TransactionId={TransactionId}, Status=Pending", transaction.Id);

                return new CreatePaymentResponse
                {
                    Success = true,
                    Message = "Tao link thanh toan thanh cong",
                    CheckoutUrl = result.CheckoutUrl,
                    OrderCode = orderCode,
                    TransactionId = transaction.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS createPaymentLink failed. UserId={UserId}, Error={Error}", userId, ex.Message);
                return Fail($"Loi khi tao link thanh toan: {ex.Message}");
            }
        }

        public async Task<bool> HandleWebhookAsync(BussinessObjects.DTOs.Payment.PayOSWebhookPayload payload)
        {
            _logger.LogInformation("Webhook START - Code={Code}, Success={Success}, OrderCode={OrderCode}",
                payload.Code, payload.Success, payload.Data?.OrderCode);

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

                _logger.LogInformation("Verifying webhook signature...");
                await _payOS.Webhooks.VerifyAsync(webhookBody);
                _logger.LogInformation("Webhook signature verified");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Webhook signature invalid. OrderCode={OrderCode}. Error={Error}",
                    payload.Data?.OrderCode, ex.Message);
                return false;
            }

            if (payload.Data is null)
            {
                _logger.LogWarning("Webhook data is null");
                return false;
            }

            long orderCode = payload.Data.OrderCode;

            var transaction = await _db.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

            if (transaction is null)
            {
                _logger.LogWarning("Transaction not found. OrderCode={OrderCode}", orderCode);
                return false;
            }

            _logger.LogInformation("Found transaction. CurrentStatus={Status}, UserId={UserId}",
                transaction.Status, transaction.UserId);

            if (transaction.Status == "Paid")
            {
                _logger.LogInformation("Transaction already paid. OrderCode={OrderCode}", orderCode);
                return true;
            }

            if (payload.Code == "00" && payload.Success)
            {
                _logger.LogInformation("Payment successful. Updating transaction...");
                transaction.Status = "Paid";
                transaction.PaidAt = DateTime.UtcNow;

                if (transaction.User is not null)
                {
                    transaction.User.CurrencyAmount += transaction.CurrencyAwarded;
                    _logger.LogInformation("Credited {VP} VP to UserId={UserId}", transaction.CurrencyAwarded, transaction.UserId);
                }
            }
            else
            {
                _logger.LogWarning("Payment failed. Code={Code}, Success={Success}", payload.Code, payload.Success);
                transaction.Status = "Failed";
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Webhook SUCCESS - Transaction updated. Status={Status}", transaction.Status);
            return true;
        }

        public async Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId)
        {
            _logger.LogInformation("GetUserTransactions: UserId={UserId}", userId);
            var transactions = await _db.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => ToDto(t))
                .ToListAsync();

            _logger.LogInformation("Found {Count} transactions", transactions.Count);
            return transactions;
        }

        public async Task<TransactionDto?> GetTransactionByOrderCodeAsync(long orderCode)
        {
            _logger.LogInformation("GetTransactionByOrderCode START - OrderCode={OrderCode}", orderCode);

            var t = await _db.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (t is null)
            {
                _logger.LogWarning("Transaction not found. OrderCode={OrderCode}", orderCode);
                return null;
            }

            _logger.LogInformation("Current status: Status={Status}", t.Status);

            if (t.Status == "Pending")
            {
                _logger.LogInformation("Status is Pending, polling PayOS...");

                try
                {
                    var paymentInfo = await _payOS.PaymentRequests.GetAsync(orderCode);
                    string? paymentStatus = paymentInfo?.Status.ToString();

                    _logger.LogInformation("PayOS response: Status={PaymentStatus}", paymentStatus);

                    if (string.Equals(paymentStatus, "PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("PayOS confirms PAID. Updating database...");
                        t.Status = "Paid";
                        t.PaidAt = DateTime.UtcNow;

                        if (t.User is not null)
                        {
                            t.User.CurrencyAmount += t.CurrencyAwarded;
                            _logger.LogInformation("Credited {VP} VP to UserId={UserId}", t.CurrencyAwarded, t.UserId);
                        }

                        await _db.SaveChangesAsync();
                        _logger.LogInformation("Database updated successfully");
                    }
                    else if (string.Equals(paymentStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("PayOS confirms CANCELLED");
                        t.Status = "Cancelled";
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogInformation("PayOS status: {PaymentStatus} (not final yet)", paymentStatus);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not poll PayOS. OrderCode={OrderCode} Error={Error}",
                        orderCode, ex.Message);
                }
            }

            _logger.LogInformation("GetTransactionByOrderCode SUCCESS");
            return ToDto(t);
        }

        public async Task<decimal> GetUserBalanceAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId);
            return user?.CurrencyAmount ?? 0;
        }

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
