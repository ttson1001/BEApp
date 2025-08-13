using BEAPI.Dtos.Common;
using BEAPI.Dtos.Wallet;
using BEAPI.Helper;
using BEAPI.PaymentService.VnPay;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly VNPayService _vnPayService;

        public WalletController(IWalletService walletService, VNPayService vnPayService)
        {
            _walletService = walletService;
            _vnPayService = vnPayService;
        }

        // Direct TopUp is not exposed; use TopUpByVnPay to initiate VNPay flow

        [HttpPost("[action]")]
        public async Task<IActionResult> Withdraw([FromBody] WalletAmountDto request)
        {
            try
            {
                var userId = GuidHelper.ParseOrThrow(request.UserId, nameof(request.UserId));
                var balance = await _walletService.Withdraw(userId, request.Amount);
                return Ok(new ResponseDto { Message = "Withdraw successful", Data = new { Balance = balance } });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpPost("[action]")]
        public IActionResult TopUpByVnPay([FromBody] WalletAmountDto request)
        {
            try
            {
                var userId = GuidHelper.ParseOrThrow(request.UserId, nameof(request.UserId));
                var url = _vnPayService.VNPayWalletTopUp(HttpContext, userId, request.Amount);
                return Ok(new ResponseDto { Message = "VNPay URL generated", Data = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }
    }
}


