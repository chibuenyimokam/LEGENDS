using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data.Response_Table;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.VAS.Request;
using LegendPay.Models.ViewModels.UserDashboard;
using LegendPay.Models.WalletStation.Request;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LegendPay.Services.Transaction
{
    public class BillPaymentHandler : IBillPaymentHandler
    {
        private readonly IAuthService _authService;
        private readonly IWalletService _walletService;
        private readonly IVasService _vasService;
        private readonly ILegendPointService _legendPointService;
        private readonly AppDbContext _context;
        private readonly ILogger<BillPaymentHandler> _logger;

        public BillPaymentHandler(
            IAuthService authService,
            IWalletService walletService,
            IVasService vasService,
            ILegendPointService legendPointService,
            AppDbContext context,
            ILogger<BillPaymentHandler> logger)
        {
            _authService = authService;
            _walletService = walletService;
            _vasService = vasService;
            _legendPointService = legendPointService;
            _context = context;
            _logger = logger;
        }

        public async Task<PayBillsViewModel> PreparePayBillsViewModelAsync(Guid userId)
        {
            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(userId, 0);
            var groupsResponse = await _vasService.GetBillerGroupsAsync();

            return new PayBillsViewModel
            {
                AvailableBalance = wallet?.Balance ?? 0m,
                RecentFavorites = new List<RecentBillerViewModel>(),
                Categories = groupsResponse?.ResponseData?
                    .Select(g => new BillerCategoryViewModel
                    {
                        Category = g.Slug,
                        LogoUrl = string.Empty,
                        IconName = MapCategoryToIcon(g.Slug)
                    })
                    .ToList() ?? new List<BillerCategoryViewModel>()
            };
        }

        public async Task<SelectBillerViewModel> PrepareSelectBillerViewModelAsync(string category)
        {
            string targetGroupSlug = MapToVasGroupSlug(category);
            var billersResponse = await _vasService.GetBillersByGroupSlugAsync(targetGroupSlug);
            var billers = billersResponse?.ResponseData?
                .Select(b => new BillerItem
                {
                    BillerId = b.Id.ToString(),
                    BillerName = b.Name,
                    Description = b.Slug,
                    LogoUrl = string.Empty,
                    ReferenceIdVerifiable = !b.SkipValidation,
                    AmountInVerification = false
                })
                .ToList() ?? new List<BillerItem>();

            return new SelectBillerViewModel
            {
                Category = category,
                CategoryDisplayName = GetCategoryDisplayName(category),
                CategoryIcon = MapCategoryToIcon(category),
                Billers = billers
            };
        }

        public async Task<BillerDetailsViewModel?> PrepareBillerDetailsViewModelAsync(
            string email, string category, string billerId, string billerName,
            bool referenceIdVerifiable, bool amountInVerification)
        {
            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null) return null;
            if (!int.TryParse(billerId, out var billerIdInt))
            {
                return null; 
            }
            var packagesResponse = await _vasService.GetPackagesByBillerIdAsync(billerIdInt);
            var packages = packagesResponse?.ResponseData?
                .Select(p => new BillerPackageItem
                {
                    Label = p.Name,
                    BillerItemId = p.Slug,
                    Amount = p.Amount ?? 0m
                })
                .ToList() ?? new List<BillerPackageItem>();

            return new BillerDetailsViewModel
            {
                Category = category,
                CategoryDisplayName = GetCategoryDisplayName(category),
                BillerId = billerId,
                BillerName = billerName,
                CustomerName = $"{user.FirstName} {user.LastName}",
                BillerLocation = category?.ToUpper() == "DISCO" ? GetElectricityLocation(billerName) : string.Empty,
                ReferenceIdVerifiable = referenceIdVerifiable,
                AmountInVerification = amountInVerification,
                Packages = packages
            };
        }

        public async Task<PaymentResult> ProcessBillPaymentAsync(string userEmail, ReviewAndPayViewModel model)
        {
            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return new PaymentResult { IsSuccess = false, ErrorMessage = "User not found." };

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            if ((wallet?.Balance ?? 0m) < model.Amount)
            {
                return new PaymentResult { IsSuccess = false, ErrorMessage = "Your wallet balance is not sufficient for this transaction. Please fund your wallet and try again." };
            }

            var paymentRef = $"LP-{Guid.NewGuid():N}"[..18].ToUpper();

            var debitRequest = new DebitRequest
            {
                Amount = model.Amount,
                CustomerId = user.CustomerId ?? string.Empty,
                Description = $"Bill payment - {model.BillerName} - {model.Category}",
                TraceId = paymentRef
            };

            var debitResponse = await _walletService.DebitWalletAsync(debitRequest);
            if (debitResponse?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
            {
                return new PaymentResult { IsSuccess = false, ErrorMessage = "Payment could not be processed at this time. Please try again." };
            }

            var localUser = await _context.UserAccounts.FirstOrDefaultAsync(u => u.CustomerId == user.CustomerId);
            if (localUser != null)
            {
                localUser.Balance = debitResponse.Balance;
                await _context.SaveChangesAsync();
            }

            var vendRequest = new VendValueRequest
            {
                PaymentReference = paymentRef,
                CustomerId = model.ReferenceNumber,
                PackageSlug = model.PackageSlug, 
                Channel = "WEB",
                Amount = model.Amount,
                CustomerName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Email = user.Email ?? string.Empty,
                AccountNumber = user.AccountNumber ?? string.Empty
            };

            var vasResponse = await _vasService.VendValueAsync(vendRequest);
            if (vasResponse == null || vasResponse.Error || vasResponse.ResponseCode != "00")
            {
                _logger.LogError("VAS Vend Failed. Code: {Code}, Message: {Msg}", vasResponse?.ResponseCode, vasResponse?.Message);

                // Vend failed after the wallet was already debited, reverse the debit so the
                // customer isn't charged for a service that was never delivered.
                var reversalRequest = new DebitReversalRequest
                {
                    CustomerId = user.CustomerId ?? string.Empty,
                    Description = "Reversal of debit",
                    TransactionId = debitResponse.TransactionId
                };

                var reversalResponse = await _walletService.DebitReversalAsync(reversalRequest);
                if (reversalResponse?.ResponseHeader?.ResponseCode == ResponseCode.Successful)
                {
                    var localUserForReversal = await _context.UserAccounts.FirstOrDefaultAsync(u => u.CustomerId == user.CustomerId);
                    // Convert Balance string to decimal safely for reversal
                    if (localUserForReversal != null && decimal.TryParse(reversalResponse.Balance, out var reversalBalance))
                    {
                        localUserForReversal.Balance = reversalBalance;
                        await _context.SaveChangesAsync();
                    }
                    
                }
                else
                {
                    // Debit succeeded, vend failed, AND the reversal failed - this needs
                    // manual reconciliation. Log loudly so it isn't missed.
                    _logger.LogCritical(
                        "REVERSAL FAILED for {PaymentRef}. User {CustomerId} was debited {Amount} but vend and reversal both failed.",
                        paymentRef, user.CustomerId, model.Amount);
                }

                return new PaymentResult { IsSuccess = false, ErrorMessage = vasResponse?.Message ?? "Bill vending failed. Please contact support." };
            }

            string? token = vasResponse.ResponseData?.TokenData?.StdToken?.Value;
            decimal.TryParse(vasResponse.ResponseData?.TokenData?.StdToken?.Units, out var unitValue);

            var successDesc = string.IsNullOrWhiteSpace(vasResponse.ResponseData?.CustomerMessage)
                ? $"Paid to {model.BillerName}"
                : vasResponse.ResponseData.CustomerMessage;

            var bill = new Bill
            {
                UserAccountId = user.Id,
                BillerCategory = model.Category ?? string.Empty,
                BillerName = model.BillerName,
                AccountReference = model.ReferenceNumber,
                Amount = model.Amount,
                PaymentMethod = "Wallet",
                Status = "Success",
                BilleroneRefrence = vasResponse.ResponseData?.TransactionId ?? paymentRef
            };
            _context.Bills.Add(bill);

            var now = DateTime.UtcNow;
            var spending = await _context.SpendingRecords.FirstOrDefaultAsync(s =>
                s.UserAccountId == user.Id && s.BillerCategory == bill.BillerCategory && s.Month == now.Month && s.Year == now.Year);
            if (spending == null)
            {
                _context.SpendingRecords.Add(new SpendingRecord
                {
                    UserAccountId = user.Id,
                    BillerCategory = bill.BillerCategory,
                    Month = now.Month,
                    Year = now.Year,
                    TotalSpent = model.Amount,
                    TransactionCount = 1
                });
            }
            else
            {
                spending.TotalSpent += model.Amount;
                spending.TransactionCount += 1;
                spending.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            var pointsEarned = await _legendPointService.AwardPointsAsync(user.Id, model.Amount, bill.Id);

            var successViewModel = new PaymentSuccessViewModel
            {
                Category = model.Category ?? string.Empty,
                BillerName = model.BillerName,
                ReferenceNumber = model.ReferenceNumber,
                ReferenceLabel = GetReferenceLabel(model.Category ?? string.Empty, model.BillerName),
                PlanLabel = model.PlanLabel,
                PlanDuration = model.PlanDuration,
                SuccessDescription = successDesc,
                Amount = model.Amount,
                PaidAt = DateTime.Now,
                TransactionRef = vasResponse.ResponseData?.TransactionId ?? paymentRef,
                PointsEarned = pointsEarned,
                IsFourStep = model.IsFourStep,
                ElectricityToken = token,
                UnitValue = unitValue
            };

            return new PaymentResult { IsSuccess = true, SuccessViewModel = successViewModel };
        }

        public async Task<PurchaseDetailsViewModel?> PreparePurchaseDetailsViewModelAsync(string email, string category, string mode)
        {
            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null) return null;

            List<BillerItem> billers = new();

            
            string targetGroupSlug = MapToVasGroupSlug(category);

            // Fetch billers from CoralPay VAS using the category/group slug
            if (!string.IsNullOrWhiteSpace(targetGroupSlug))
            {
                var billersResponse = await _vasService.GetBillersByGroupSlugAsync(targetGroupSlug);
                _logger.LogInformation("Billers for group '{TargetGroup}': {Count} results",
                    targetGroupSlug, billersResponse?.ResponseData?.Count ?? 0);
                
                billers = billersResponse?.ResponseData?
                    .Select(b => new BillerItem
                    {
                        BillerId = b.Id.ToString(),
                        BillerName = b.Name,
                        Description = b.Slug,
                        LogoUrl = string.Empty,
                        ReferenceIdVerifiable = !b.SkipValidation,
                        AmountInVerification = false
                    })
                    .ToList() ?? new List<BillerItem>();
            }

            return new PurchaseDetailsViewModel
            {
                Category = category ?? string.Empty,
                Mode = string.IsNullOrWhiteSpace(mode) ? category : mode, 
                CategoryDisplayName = GetCategoryDisplayName(category ?? string.Empty),
                CategoryIcon = MapCategoryToIcon(category ?? string.Empty),
                Billers = billers,
                CustomerName = $"{user.FirstName} {user.LastName}"
            };
        }
        public async Task<ReviewAndPayViewModel?> PrepareReviewAndPayViewModelAsync(
        string email, string category, string billerId, string billerName, string packageSlug,
        string referenceNumber, string customerName, string planLabel,
        string planDuration, decimal amount, bool saveBeneficiary)
        {
            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null) return null;

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            return new ReviewAndPayViewModel
            {
                Category = category,
                CategoryDisplayName = GetCategoryDisplayName(category),
                CategoryIcon = MapCategoryToIcon(category),
                BillerId = billerId,
                BillerName = billerName,
                PackageSlug = packageSlug,
                ReferenceNumber = referenceNumber,
                ReferenceLabel = GetReferenceLabel(category, billerName),
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? $"{user.FirstName} {user.LastName}" : customerName,
                PlanLabel = planLabel,
                PlanDuration = planDuration,
                Amount = amount,
                WalletBalance = wallet?.Balance ?? 0m,
                SaveBeneficiary = saveBeneficiary,
                IsFourStep = category?.ToUpper() is "DISCO" or "ELECTRICITY" or "DIGITALTV" or "EDUCATION"
            };
        }

        public async Task<List<BillerPackageItem>> GetBillerPackagesAsync(int billerId)
        {
            var packagesResponse = await _vasService.GetPackagesByBillerIdAsync(billerId);
            return packagesResponse?.ResponseData?
                .Select(p => new BillerPackageItem
                {
                    Label = p.Name,
                    BillerItemId = p.Slug,
                    Amount = p.Amount ?? 0m
                })
                .ToList() ?? new List<BillerPackageItem>();
        }

        // Helper Methods
        private string MapCategoryToIcon(string category) => category?.ToUpper() switch
        {
            "ELECTRICITY" or "ELECTRIC_DISCO" or "DISCO" => "bolt",
            "AIRTIME" or "DATA" or "AIRTIME_AND_DATA" => "smartphone",
            "GAMES" or "BETTING" or "BETTING_AND_LOTTERY" => "sports_esports",
            "DIGITALTV" or "PAY_TV" => "tv",
            "TRANSPORT" or "TRANSPORT_AND_TOLL_PAYMENT" => "directions_car",
            "EDUCATION" => "school",
            _ => "receipt_long"
        };

        private string GetCategoryDisplayName(string category) => category?.ToUpper() switch
        {
            "ELECTRICITY" or "ELECTRIC_DISCO" or "DISCO" => "Electricity",
            "AIRTIME" => "Airtime",
            "DATA" => "Data",
            "AIRTIME_AND_DATA" => "Airtime & Data",
            "GAMES" or "BETTING" or "BETTING_AND_LOTTERY" => "Betting & Games",
            "DIGITALTV" or "PAY_TV" => "Digital TV",
            "TRANSPORT" or "TRANSPORT_AND_TOLL_PAYMENT" => "Transport & Tolls",
            "EDUCATION" => "Education",
            _ => category ?? "Bills"
        };
        private string MapToVasGroupSlug(string category) => category?.ToUpper() switch
        {
            "AIRTIME" or "DATA" or "AIRTIME_AND_DATA" => "AIRTIME_AND_DATA",
            "ELECTRICITY" or "DISCO" or "ELECTRIC_DISCO" => "ELECTRIC_DISCO",
            "DIGITALTV" or "PAY_TV" => "PAY_TV",
            "GAMES" or "BETTING" or "BETTING_AND_LOTTERY" => "BETTING_AND_LOTTERY",
            "TRANSPORT" or "TRANSPORT_AND_TOLL_PAYMENT" => "TRANSPORT_AND_TOLL_PAYMENT",
            "EDUCATION" => "EDUCATION",
            _ => category ?? string.Empty
        };

        private string GetReferenceLabel(string category, string billerName) => category?.ToUpper() switch
        {
            "DISCO" => "Meter / Account Number",
            "AIRTIME_AND_DATA" => "Phone Number",
            "BETTING_AND_LOTTERY" => "User ID",
            _ => "Reference Number"
        };

        private string GetElectricityLocation(string billerName) => billerName?.ToUpper() switch
        {
            var b when b.Contains("EKEDC") || b.Contains("IKEDC") => "Lagos, Nigeria",
            var b when b.Contains("AEDC") => "Abuja, Nigeria",
            var b when b.Contains("IBEDC") => "Ibadan, Nigeria",
            var b when b.Contains("EEDC") => "Enugu, Nigeria",
            var b when b.Contains("PHEDC") => "Port Harcourt, Nigeria",
            _ => "Nigeria"
        };
    }
}