using Microsoft.EntityFrameworkCore;

namespace MULTI_Bet.Infrastructure.Wallet;

public sealed class WalletRepository(WalletDbContext db) : IWalletRepository
{
    public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Wallets.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default) =>
        db.Wallets.SingleOrDefaultAsync(x => x.Id == walletId, cancellationToken);

    public Task<WalletTransaction?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
        db.WalletTransactions.SingleOrDefaultAsync(x => x.Id == transactionId, cancellationToken);

    public Task<WalletTransaction?> GetByPixTxIdAsync(string pixTxId, CancellationToken cancellationToken = default) =>
        db.WalletTransactions.SingleOrDefaultAsync(x => x.PixTxId == pixTxId, cancellationToken);

    public async Task<IReadOnlyList<WalletTransaction>> GetTransactionsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var transactions = await db.WalletTransactions
            .Where(x => x.WalletId == walletId)
            .ToListAsync(cancellationToken);
        return transactions.OrderBy(x => x.CreatedAt).ToArray();
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default) =>
        await db.Wallets.AddAsync(wallet, cancellationToken);

    public async Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default) =>
        await db.WalletTransactions.AddAsync(transaction, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
