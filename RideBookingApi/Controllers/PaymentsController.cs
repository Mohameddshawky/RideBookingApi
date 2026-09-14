using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Payments.ProcessPayment;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ProcessPaymentHandler _processPaymentHandler;

    public PaymentsController(ProcessPaymentHandler processPaymentHandler)
    {
        _processPaymentHandler = processPaymentHandler;
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResultDto>> ProcessPayment([FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _processPaymentHandler.HandleAsync(command, cancellationToken);

        if (result.Status == RideBookingApi.Domain.Enums.PaymentStatus.Failed)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
