namespace MULTI_Bet.Shared.Wallet;

public sealed record WalletBalanceDto(decimal Available, decimal Pending);

public enum WalletTransactionType
{
    PixDeposit = 1,
    PixWithdrawal = 2,
    BetDebit = 3,
    BetCredit = 4,
    Adjustment = 5
}

public enum WalletTransactionStatus
{
    Pending = 1,
    Confirmed = 2,
    Failed = 3,
    Cancelled = 4
}

public sealed record WalletTransactionDto(
    Guid Id,
    WalletTransactionType Type,
    WalletTransactionStatus Status,
    decimal Amount,
    string Currency,
    string? PixTxId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt);

public sealed record CreatePixDepositRequest(decimal Amount);

public sealed record CreatePixDepositResponse(
    Guid TransactionId,
    string Status,
    decimal Amount,
    string? PixCopyPaste,
    string? QrCodeBase64);
