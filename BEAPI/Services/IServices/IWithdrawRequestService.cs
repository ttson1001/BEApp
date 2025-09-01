using BEAPI.Dtos.Withdraw;

namespace BEAPI.Services
{
    public interface IWithdrawRequestService
    {
        Task<WithdrawRequestDto> CreateAsync(CreateWithdrawRequestDto dto, Guid userId);
        Task<List<WithdrawRequestDto>> GetUserRequestsAsync(Guid userId);
        Task<List<WithdrawRequestDto>> GetAllRequestsAsync();
        Task<bool> ApproveAsync(Guid requestId);
        Task<bool> RejectAsync(Guid requestId, string reason);
    }
}
