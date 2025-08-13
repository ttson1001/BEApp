using BEAPI.Dtos.Common;
using BEAPI.Dtos.Payment;

namespace BEAPI.Services.IServices
{
    public interface IPaymentHistoryService
    {
        Task<PagedResult<PaymentHistoryDto>> SearchAsync(PaymentHistorySearchDto request);
    }
}


