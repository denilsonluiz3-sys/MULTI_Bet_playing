using MULTI_Bet.Shared.Wallet;

namespace MULTI_Bet.Infrastructure.Wallet;

public sealed class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Currency { get; set; } = "BRL";
}

public sealed class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public WalletTransactionType Type { get; set; }
    public WalletTransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BRL";
    public string? PixTxId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}
