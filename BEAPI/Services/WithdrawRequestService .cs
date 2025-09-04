using BEAPI.Dtos.Withdraw;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class WithdrawRequestService : IWithdrawRequestService
    {
        private readonly IRepository<WithdrawRequest> _withdrawRepo;
        private readonly IRepository<PaymentHistory> _paymentHistoryRepo;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IUserService _userService;

        public WithdrawRequestService(IUserService userService, IRepository<WithdrawRequest> withdrawRepo, IRepository<Wallet> walletRepo, IRepository<PaymentHistory> paymentHistoryRepo)
        {
            _withdrawRepo = withdrawRepo;
            _paymentHistoryRepo = paymentHistoryRepo;
            _walletRepo = walletRepo;
            _userService = userService;
        }

        public async Task<WithdrawRequestDto> CreateAsync(CreateWithdrawRequestDto dto, Guid userId)
        {
            var request = new WithdrawRequest
            {
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                AccountHolder = dto.AccountHolder,
                Note = dto.Note,
                Amount = dto.Amount,
                UserId = userId,
                Status = WithdrawStatus.Pending,
            };

            await _withdrawRepo.AddAsync(request);
            await _withdrawRepo.SaveChangesAsync();

            return new WithdrawRequestDto
            {
                Id = request.Id,
                BankName = request.BankName,
                BankAccountNumber = request.BankAccountNumber,
                AccountHolder = request.AccountHolder,
                Note = request.Note,
                Amount = request.Amount,
                Status = request.Status,
                CreatedAt = request.CreationDate,
            };
        }

        public async Task<List<WithdrawRequestDto>> GetUserRequestsAsync(Guid userId)
        {
            return await _withdrawRepo.Get()
                .Where(x => x.UserId == userId)
                .Select(x => new WithdrawRequestDto
                {
                    Id = x.Id,
                    BankName = x.BankName,
                    BankAccountNumber = x.BankAccountNumber,
                    AccountHolder = x.AccountHolder,
                    Note = x.Note,
                    Amount = x.Amount,
                    Status = x.Status,
                    CreatedAt = x.CreationDate,
                })
                .ToListAsync();
        }

        public async Task<List<WithdrawRequestDto>> GetAllRequestsAsync()
        {
            return await _withdrawRepo.Get()
                .Select(x => new WithdrawRequestDto
                {
                    Id = x.Id,
                    BankName = x.BankName,
                    BankAccountNumber = x.BankAccountNumber,
                    AccountHolder = x.AccountHolder,
                    Note = x.Note,
                    Amount = x.Amount,
                    Status = x.Status,
                    CreatedAt = x.CreationDate,
                })
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(Guid requestId)
        {
            var request = await _withdrawRepo.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null)
                throw new InvalidOperationException($"Withdraw request {requestId} not found.");

            if (request.Status == WithdrawStatus.Approved)
                throw new InvalidOperationException($"Withdraw request {requestId} is already approved.");

            var wallet = await _walletRepo.Get().FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (wallet == null)
                throw new InvalidOperationException($"Wallet for user {request.UserId} not found.");

            if (wallet.Amount < request.Amount)
                throw new InvalidOperationException("Insufficient balance in wallet.");

            request.Status = WithdrawStatus.Approved;
            wallet.Amount -= request.Amount;

            var payment = new PaymentHistory
            {
                UserId = request.UserId,
                Amount = request.Amount,
                PaymentMenthod = "WALLET",
                PaymentStatus = PaymentStatus.Withdraw
            };
            
            _withdrawRepo.Update(request);
            _walletRepo.Update(wallet);
            await _paymentHistoryRepo.AddAsync(payment);
            await _withdrawRepo.SaveChangesAsync();
            await _userService.SendNotificationToUserAsync(request.UserId, "Silver Cart", "Đơn rút tiền của bạn đã được chấp thuận");
            return true;
        }

        public async Task<bool> RejectAsync(Guid requestId, string reason)
        {
            var request = await _withdrawRepo.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null) return false;

            request.Status = WithdrawStatus.Rejected;
            _withdrawRepo.Update(request);
            await _withdrawRepo.SaveChangesAsync();
            await _userService.SendNotificationToUserAsync(request.UserId, "Silver Cart", $"Đơn rút tiền của bạn đã bị từ chối vs lí do {reason}");
            return true;
        }
    }
}
