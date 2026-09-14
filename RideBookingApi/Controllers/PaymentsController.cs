using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("PaymentPolicy")]
    public async Task<ActionResult<PaymentResultDto>> ProcessPayment([FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _processPaymentHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }
}
