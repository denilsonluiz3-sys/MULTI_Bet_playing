using MULTI_Bet.Shared.Wallet;

namespace MULTI_Bet.Infrastructure.Wallet;

public sealed class WalletService
{
    private readonly IWalletRepository _repository;

    public WalletService(IWalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
            return new WalletBalanceDto(0m, 0m);

        decimal available = 0m, pending = 0m;
        foreach (var t in await _repository.GetTransactionsAsync(wallet.Id, cancellationToken))
        {
            if (t.Status == WalletTransactionStatus.Confirmed)
                available += IsCredit(t.Type) ? t.Amount : -t.Amount;
            else if (t.Status == WalletTransactionStatus.Pending && t.Type == WalletTransactionType.PixDeposit)
                pending += t.Amount;
        }

        return new WalletBalanceDto(available, pending);
    }

    public async Task<IReadOnlyList<WalletTransactionDto>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
            return Array.Empty<WalletTransactionDto>();

        return (await _repository.GetTransactionsAsync(wallet.Id, cancellationToken))
            .Select(ToDto)
            .ToArray();
    }

    public async Task<CreatePixDepositResponse> CreatePixDepositAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "amount_must_be_positive");

        var wallet = await GetOrCreateWalletAsync(userId, cancellationToken);
        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Type = WalletTransactionType.PixDeposit,
            Status = WalletTransactionStatus.Pending,
            Amount = amount,
            Currency = wallet.Currency,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CreatePixDepositResponse(
            transaction.Id,
            "pending",
            transaction.Amount,
            MockPixCopyPaste(transaction.Id),
            null);
    }

    public async Task<WalletTransactionDto?> ConfirmPixDepositAsync(Guid transactionId, string pixTxId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pixTxId))
            throw new ArgumentException("pix_tx_id_required", nameof(pixTxId));

        var transaction = await _repository.GetTransactionAsync(transactionId, cancellationToken);
        if (transaction is null)
            return null;

        if (transaction.Status == WalletTransactionStatus.Confirmed)
        {
            if (string.Equals(transaction.PixTxId, pixTxId, StringComparison.Ordinal))
                return ToDto(transaction);

            throw new InvalidOperationException("transaction_already_confirmed");
        }

        var duplicate = await _repository.GetByPixTxIdAsync(pixTxId, cancellationToken);
        if (duplicate is not null && duplicate.Id != transactionId)
            throw new InvalidOperationException("pix_tx_id_already_used");

        transaction.Status = WalletTransactionStatus.Confirmed;
        transaction.PixTxId = pixTxId;
        transaction.ConfirmedAt = DateTimeOffset.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    private async Task<Wallet> GetOrCreateWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is not null)
            return wallet;

        wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Currency = "BRL" };
        await _repository.AddAsync(wallet, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    private static bool IsCredit(WalletTransactionType type) =>
        type is WalletTransactionType.PixDeposit or WalletTransactionType.BetCredit or WalletTransactionType.Adjustment;

    private static WalletTransactionDto ToDto(WalletTransaction t) =>
        new(t.Id, t.Type, t.Status, t.Amount, t.Currency, t.PixTxId, t.CreatedAt, t.ConfirmedAt);

    private static string MockPixCopyPaste(Guid transactionId) =>
        $"00020126580014BR.GOV.BCB.PIX0136MOCK-{transactionId:N}5204000053039865802BR5913MULTI BET6009SAO PAULO62070503***63040000";
}