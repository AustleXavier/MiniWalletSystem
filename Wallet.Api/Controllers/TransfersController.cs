using Microsoft.AspNetCore.Mvc;
using Wallet.Api;
using Wallet.Application;
using Wallet.Application.Features.Wallets.Commands;

namespace Wallet.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public sealed class TransfersController(TransferWalletHandler transferWallet) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletResponse>> Create(TransferRequest request, CancellationToken ct) 
        => Ok(await transferWallet.Handle(new(request.FromWalletId, request.ToWalletId, request.Amount, request.ReferenceId), ct));
}
