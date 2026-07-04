using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Wallets.Create;
using Wallet.Application.Wallets.Deposit;
using Wallet.Application.Wallets.GetById;
using Wallet.Application.Wallets.Withdraw;

namespace Wallet.Api.Controller;

[Route("api/[controller]")]
[ApiController]
public class WalletsController : ControllerBase
{
  private readonly IMediator _mediator;

  public WalletsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost(nameof(Create))]
  public async Task<IActionResult> Create([FromBody] CreateWalletCommand request)
  {
    var result = await _mediator.Send(request);

    if (result.IsSuccess)
    {
      return Ok(result);
    }
    return BadRequest(result);
  }

  [HttpPost(nameof(Deposit))]
  public async Task<IActionResult> Deposit([FromBody] DepositCommand request)
  {
    var result = await _mediator.Send(request);

    if (result.IsSuccess)
    {
      return Ok(result);
    }
    return BadRequest(result);
  }

  [HttpPost(nameof(Withdraw))]
  public async Task<IActionResult> Withdraw([FromBody] WithdrawCommand request)
  {
    var result = await _mediator.Send(request);

    if (result.IsSuccess)
    {
      return Ok(result);
    }
    return BadRequest(result);
  }

  [HttpGet(nameof(GetById))]
  public async Task<IActionResult> GetById([FromQuery] Guid guid)
  {
    var result = await _mediator.Send(new GetWalletByIdQuery(guid));

    if (result.IsSuccess)
    {
      return Ok(result);
    }
    return BadRequest(result);
  }
}
