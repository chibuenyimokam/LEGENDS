using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data.Response_Table;
using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;
using Microsoft.AspNetCore.Mvc;

namespace LegendPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<TransactionController> _logger;
        private readonly AppDbContext _context;

        public TransactionController(
            IWalletService walletService,
            ILogger<TransactionController> logger,
            AppDbContext context)
        {
            _walletService = walletService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("credit")]
        public async Task<IActionResult> CreditWallet([FromBody] CreditRequest request)
        {
            if (request == null || request.Amount <= 0 || string.IsNullOrEmpty(request.CustomerId))
            {
                return BadRequest("Invalid credit request data.");
            }

            //Sends credit to CoralPay
            var creditResponse = await _walletService.CreditWalletAsync(request);

            if (creditResponse?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
            {
                return BadRequest("Credit operation failed at CoralPay: " + creditResponse?.ResponseHeader?.ResponseCode);
            }

            //var freshBalance = await _walletService.GetBalanceAsync(request.CustomerId);



            //if (!freshBalance.HasValue)
            //{
            //    return StatusCode(500, "Credit succeeded but failed to fetch updated balance.");
            //}

            var localUser = _context.UserAccounts.FirstOrDefault(u => u.CustomerId == request.CustomerId);

            if (localUser != null)
            {
                localUser.Balance = creditResponse.Balance;
                await _context.SaveChangesAsync();
            }
            

            return Ok(new
            {
                message = "Credit successful",
                newBalance = creditResponse.Balance,
                coralPayResponse = creditResponse
            });
        }


        [HttpPost("debit")]
        
        public async Task<IActionResult> DebitWallet([FromBody] DebitRequest request)
        {
            if (request == null || request.Amount <= 0 || string.IsNullOrEmpty(request.CustomerId))
            {
                return BadRequest("Invalid Debit request data.");
            }
            //Sends debit to CoralPay
            var debitResponse = await _walletService.DebitWalletAsync(request);

            if (debitResponse?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
            {
                return BadRequest("Debit operation failed at CoralPay: " + debitResponse?.ResponseHeader?.ResponseCode);
            }

            var localUser = _context.UserAccounts.FirstOrDefault(u => u.CustomerId == request.CustomerId);

            if (localUser != null)
            {
                localUser.Balance = debitResponse.Balance;
                await _context.SaveChangesAsync();
            }


            return Ok(new
            {
                message = "Debit successful",
                newBalance = debitResponse.Balance,
                coralPayResponse = debitResponse
            });
        }
    }
}