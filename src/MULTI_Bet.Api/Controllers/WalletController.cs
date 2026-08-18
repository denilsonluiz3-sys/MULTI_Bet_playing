using Microsoft.AspNetCore.Mvc;
using MULTI_Bet.Shared.Wallet;

namespace MULTI_Bet.Api.Controllers;

[ApiController]
[Route("api/wallet")]
public sealed class WalletController : ControllerBase
{
    [HttpGet("balance")]
    public ActionResult<WalletBalanceDto> GetBalance()
        => Ok(new WalletBalanceDto(0m, 0m));

    [HttpPost("deposits/pix")]
    public ActionResult<CreatePixDepositResponse> CreatePixDeposit([FromBody] CreatePixDepositRequest request)
    {
        if (request.Amount <= 0m)
            return BadRequest(new { error = "amount_must_be_positive" });

        return Ok(new CreatePixDepositResponse(
            Guid.NewGuid(),
            "pending",
            request.Amount,
            null,
            null));
    }
}
