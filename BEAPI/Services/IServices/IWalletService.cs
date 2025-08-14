namespace BEAPI.Services.IServices
{
    public interface IWalletService
    {
        Task<decimal> TopUp(Guid userId, decimal amount, CancellationToken ct = default);
        Task<decimal> Withdraw(Guid userId, decimal amount, CancellationToken ct = default);
        Task<decimal> GetAmount(Guid userId, CancellationToken ct = default);
    }
}
