using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BEAPI.Services
{
    public class WalletService : IWalletService
    {
        private readonly IRepository<Wallet> _repository;

        public WalletService(IRepository<Wallet> repository) {
            _repository = repository;
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
    }
}
