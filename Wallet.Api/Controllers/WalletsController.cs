using Microsoft.AspNetCore.Mvc;
using Wallet.Api;
using Wallet.Application;
using Wallet.Application.Features.Wallets.Commands;
using Wallet.Domain.Entities;

namespace Wallet.Api.Controllers;

[ApiController]
[Route("api/wallets")]
public sealed class WalletsController(IWalletService wallets, CreateWalletHandler createWallet, CreditWalletHandler creditWallet, DebitWalletHandler debitWallet) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateWalletRequest request, CancellationToken ct)
    {
        var result = await createWallet.Handle(new(request.Name, request.Email, request.MobileNumber, request.InitialBalance), ct);
        return CreatedAtAction(nameof(GetById), new { walletId = result.WalletId }, result);
    }

    [HttpGet("{walletId:guid}")]
    public async Task<ActionResult<WalletResponse>> GetById(Guid walletId, CancellationToken ct) =>
        await wallets.GetAsync(walletId, ct) is { } wallet ? Ok(wallet) : NotFound();

    [HttpPost("{walletId:guid}/credit")]
    public async Task<ActionResult<WalletResponse>> Credit(Guid walletId, MoneyRequest request, CancellationToken ct) =>
        Ok(await creditWallet.Handle(new(walletId, request.Amount, request.ReferenceId), ct));

    [HttpPost("{walletId:guid}/debit")]
    public async Task<ActionResult<WalletResponse>> Debit(Guid walletId, MoneyRequest request, CancellationToken ct) =>
        Ok(await debitWallet.Handle(new(walletId, request.Amount, request.ReferenceId), ct));

    [HttpGet("{walletId:guid}/transactions")]
    public async Task<ActionResult<PagedResponse<TransactionResponse>>> Transactions(Guid walletId, [FromQuery] TransactionType? type, [FromQuery] DateTimeOffset? fromDate, [FromQuery] DateTimeOffset? toDate, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["paging"] = new[] { "Page number must be >= 1 and page size must be 1-100." }
                })
                    {
                        Status = StatusCodes.Status400BadRequest
                    });

        return Ok(await wallets.GetHistoryAsync(walletId, type, fromDate, toDate, pageNumber, pageSize, ct));
    }
}
