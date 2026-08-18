using MULTI_Bet.Shared.Wallet;
using Xunit;

namespace MULTI_Bet.Infrastructure.Tests;

public sealed class WalletServiceTests
{
    [Fact]
    public async Task BalanceIsZeroForUnknownUser()
    {
        using var harness = new WalletTestHarness();

        var balance = await harness.Service.GetBalanceAsync(Guid.NewGuid());

        Assert.Equal(0m, balance.Available);
        Assert.Equal(0m, balance.Pending);
    }

    [Fact]
    public async Task CreateDepositCreatesPendingTransactionAndPendingBalance()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        var created = await harness.Service.CreatePixDepositAsync(userId, 100m);

        Assert.Equal("pending", created.Status);
        Assert.NotNull(created.PixCopyPaste);
        Assert.Contains("MOCK-", created.PixCopyPaste);

        var balance = await harness.Service.GetBalanceAsync(userId);
        Assert.Equal(0m, balance.Available);
        Assert.Equal(100m, balance.Pending);

        var transactions = await harness.Service.GetTransactionsAsync(userId);
        var transaction = Assert.Single(transactions);
        Assert.Equal(WalletTransactionType.PixDeposit, transaction.Type);
        Assert.Equal(WalletTransactionStatus.Pending, transaction.Status);
        Assert.Equal(100m, transaction.Amount);
    }

    [Fact]
    public async Task ConfirmDepositMovesAmountToAvailableAndIsIdempotent()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        var created = await harness.Service.CreatePixDepositAsync(userId, 100m);
        var confirmed = await harness.Service.ConfirmPixDepositAsync(created.TransactionId, "pix-tx-1");

        Assert.NotNull(confirmed);
        Assert.Equal(WalletTransactionStatus.Confirmed, confirmed.Status);
        Assert.Equal("pix-tx-1", confirmed.PixTxId);
        Assert.NotNull(confirmed.ConfirmedAt);

        var balance = await harness.Service.GetBalanceAsync(userId);
        Assert.Equal(100m, balance.Available);
        Assert.Equal(0m, balance.Pending);

        var repeated = await harness.Service.ConfirmPixDepositAsync(created.TransactionId, "pix-tx-1");
        Assert.NotNull(repeated);
        Assert.Equal(WalletTransactionStatus.Confirmed, repeated.Status);

        var balanceAfterReconfirm = await harness.Service.GetBalanceAsync(userId);
        Assert.Equal(100m, balanceAfterReconfirm.Available);
        Assert.Equal(0m, balanceAfterReconfirm.Pending);
    }

    [Fact]
    public async Task BalanceIsDerivedFromConfirmedTransactionsOnly()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        var confirmedDeposit = await harness.Service.CreatePixDepositAsync(userId, 200m);
        var pendingDeposit = await harness.Service.CreatePixDepositAsync(userId, 50m);
        await harness.Service.ConfirmPixDepositAsync(confirmedDeposit.TransactionId, "pix-tx-confirmed");

        var balance = await harness.Service.GetBalanceAsync(userId);
        Assert.Equal(200m, balance.Available);
        Assert.Equal(50m, balance.Pending);
    }

    [Fact]
    public async Task SamePixTxIdCannotBeConfirmedTwiceOnDifferentTransactions()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        var first = await harness.Service.CreatePixDepositAsync(userId, 100m);
        var second = await harness.Service.CreatePixDepositAsync(userId, 100m);
        await harness.Service.ConfirmPixDepositAsync(first.TransactionId, "pix-tx-duplicate");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => harness.Service.ConfirmPixDepositAsync(second.TransactionId, "pix-tx-duplicate"));

        Assert.Equal("pix_tx_id_already_used", exception.Message);

        var balance = await harness.Service.GetBalanceAsync(userId);
        Assert.Equal(100m, balance.Available);
    }

    [Fact]
    public async Task ConfirmingWithDifferentPixTxIdOnConfirmedTransactionThrows()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        var created = await harness.Service.CreatePixDepositAsync(userId, 100m);
        await harness.Service.ConfirmPixDepositAsync(created.TransactionId, "pix-tx-original");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => harness.Service.ConfirmPixDepositAsync(created.TransactionId, "pix-tx-other"));

        Assert.Equal("transaction_already_confirmed", exception.Message);
    }

    [Fact]
    public async Task ConfirmingUnknownTransactionReturnsNull()
    {
        using var harness = new WalletTestHarness();

        var result = await harness.Service.ConfirmPixDepositAsync(Guid.NewGuid(), "pix-tx");

        Assert.Null(result);
    }

    [Fact]
    public async Task WalletIsCreatedOncePerUser()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        var first = await harness.Service.CreatePixDepositAsync(userId, 10m);
        var second = await harness.Service.CreatePixDepositAsync(userId, 20m);

        Assert.NotEqual(first.TransactionId, second.TransactionId);

        var transactions = await harness.Service.GetTransactionsAsync(userId);
        Assert.Equal(2, transactions.Count);

        var wallets = harness.Db.Wallets.Count();
        Assert.Equal(1, wallets);
    }

    [Fact]
    public async Task CreateDepositRejectsNonPositiveAmount()
    {
        using var harness = new WalletTestHarness();
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => harness.Service.CreatePixDepositAsync(userId, 0m));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => harness.Service.CreatePixDepositAsync(userId, -5m));
    }
}