namespace MULTI_Bet.Infrastructure.Wallet;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WalletTransaction?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<WalletTransaction?> GetByPixTxIdAsync(string pixTxId, CancellationToken cancellationToken = default);
    Task AddTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);
    Task SaveAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
