using Microsoft.AspNetCore.Mvc;
using MULTI_Bet.Infrastructure.Wallet;
using MULTI_Bet.Shared.Wallet;

namespace MULTI_Bet.Api.Controllers;

[ApiController]
[Route("api/wallet")]
public sealed class WalletController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly WalletService _walletService;

    public WalletController(WalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance(
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        CancellationToken cancellationToken)
    {
        var balance = await _walletService.GetBalanceAsync(userId ?? DemoUserId, cancellationToken);
        return Ok(balance);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IReadOnlyList<WalletTransactionDto>>> GetTransactions(
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        CancellationToken cancellationToken)
    {
        var transactions = await _walletService.GetTransactionsAsync(userId ?? DemoUserId, cancellationToken);
        return Ok(transactions);
    }

    [HttpPost("deposits/pix")]
    public async Task<ActionResult<CreatePixDepositResponse>> CreatePixDeposit(
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        [FromBody] CreatePixDepositRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0m)
            return BadRequest(new { error = "amount_must_be_positive" });

        var response = await _walletService.CreatePixDepositAsync(userId ?? DemoUserId, request.Amount, cancellationToken);
        return Ok(response);
    }

    [HttpPost("transactions/{transactionId:guid}/confirm")]
    public async Task<ActionResult<WalletTransactionDto>> ConfirmPixDeposit(
        Guid transactionId,
        [FromBody] ConfirmPixDepositRequest request,
        CancellationToken cancellationToken)
    {
        WalletTransactionDto? transaction;
        try
        {
            transaction = await _walletService.ConfirmPixDepositAsync(transactionId, request.PixTxId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }

        if (transaction is null)
            return NotFound();

        return Ok(transaction);
    }
}