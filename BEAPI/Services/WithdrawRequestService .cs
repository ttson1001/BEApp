using BEAPI.Dtos.Withdraw;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class WithdrawRequestService : IWithdrawRequestService
    {
        private readonly IRepository<WithdrawRequest> _withdrawRepo;

        public WithdrawRequestService(IRepository<WithdrawRequest> withdrawRepo)
        {
            _withdrawRepo = withdrawRepo;
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

        public async Task<bool> ApproveAsync(Guid requestId)
        {
            var request = await _withdrawRepo.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null) return false;

            request.Status = WithdrawStatus.Approved;
            _withdrawRepo.Update(request);
            await _withdrawRepo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectAsync(Guid requestId, string reason)
        {
            var request = await _withdrawRepo.Get().FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null) return false;

            request.Status = WithdrawStatus.Rejected;
            _withdrawRepo.Update(request);
            await _withdrawRepo.SaveChangesAsync();

            return true;
        }
    }
}
