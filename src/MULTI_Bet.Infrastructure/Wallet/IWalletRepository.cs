namespace MULTI_Bet.Infrastructure.Wallet;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<WalletTransaction?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<WalletTransaction?> GetByPixTxIdAsync(string pixTxId, CancellationToken cancellationToken = default);
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
