using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Transfers.Create;

namespace Wallet.Api.Controller;

[Route("api/[controller]")]
[ApiController]
public class TransfersController : ControllerBase
{
  private readonly IMediator _mediator;

  public TransfersController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  public async Task<IActionResult> Transfer([FromBody] CreateTransferCommand request)
  {
    var result = await _mediator.Send(request);

    if (result.IsSuccess)
    {
      return Ok(result);
    }
    return BadRequest(result);
  }
}
