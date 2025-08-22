using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BEAPI.Services
{
    public class WalletService : IWalletService
    {
        private readonly IRepository<Wallet> _repository;
        private readonly IRepository<PaymentHistory> _paymentRepo;

        public WalletService(IRepository<Wallet> repository, IRepository<PaymentHistory> paymentRepo)
        {
            _repository = repository;
            _paymentRepo = paymentRepo;
        }
        public async Task<decimal> TopUp(Guid userId, decimal amount, CancellationToken ct = default)
        {
            if (amount <= 0)
            {
                throw new Exception("Amount must be greater than 0");
            }



            var wallet = await _repository.Get().FirstOrDefaultAsync(w => w.UserId == userId, ct);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Amount = 0
                };
                await _repository.AddAsync(wallet, ct);
            }

            wallet.Amount += amount;
            _repository.Update(wallet);
            var payment = new PaymentHistory
            {
                UserId = userId,
                Amount = amount,
                PaymentMenthod = "WALLET",
                PaymentStatus = PaymentStatus.TopUp
            };

            await _paymentRepo.AddAsync(payment, ct);
            await _repository.SaveChangesAsync(ct);
            return wallet.Amount;
        }

        public async Task<decimal> Withdraw(Guid userId, decimal amount, CancellationToken ct = default)
        {
            if (amount <= 0)
            {
                throw new Exception("Amount must be greater than 0");
            }

            var wallet = await _repository.Get().FirstOrDefaultAsync(w => w.UserId == userId, ct)
                ?? throw new Exception("Wallet not found");

                    if (wallet.Amount < amount)
                    {
                        throw new Exception("Insufficient balance");
                    }

            wallet.Amount -= amount;
            _repository.Update(wallet);
            await _repository.SaveChangesAsync(ct);
            return wallet.Amount;
        }

        public async Task<decimal> GetAmount(Guid userId, CancellationToken ct = default)
        {
            var wallet = await _repository.Get().FirstOrDefaultAsync(w => w.UserId == userId, ct);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Amount = 0
                };
                await _repository.AddAsync(wallet, ct);
                await _repository.SaveChangesAsync(ct);
            }
            return wallet.Amount;
        }
    }
}
