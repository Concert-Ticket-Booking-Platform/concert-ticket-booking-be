using ConcertTicket.Application.Payments.DTOs;
using ConcertTicket.Application.Payments.Interfaces;
using ConcertTicket.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreatePaymentResponse>> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var ipAddress =
            HttpContext.Connection.RemoteIpAddress?
                .ToString()
            ?? "127.0.0.1";

        var response =
            await _paymentService.CreateAsync(
                userId,
                request,
                ipAddress,
                cancellationToken);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("momo-ipn")]
    public async Task<IActionResult> MoMoIpn(
        [FromBody] Dictionary<string, JsonElement> body,
        CancellationToken cancellationToken)
    {
        var parameters = body.ToDictionary(
            x => x.Key,
            x => x.Value.ToString(),
            StringComparer.Ordinal);

        await _paymentService.HandleCallbackAsync(
            PaymentProvider.MoMo,
            parameters,
            cancellationToken);

        return Ok(new
        {
            success = true
        });
    }

    [AllowAnonymous]
    [HttpGet("momo-return")]
    public async Task<ActionResult<PaymentReturnResponse>> MoMoReturn(
        [FromQuery] Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var result =
            await _paymentService.HandleReturnAsync(
                PaymentProvider.MoMo,
                parameters,
                cancellationToken);

        return Ok(result);
    }

    //[AllowAnonymous]
    //[HttpGet("vnpay-return")]
    //public async Task<ActionResult<PaymentReturnResponse>> VnPayReturn(
    //[FromQuery] Dictionary<string, string> parameters,
    //CancellationToken cancellationToken)
    //{
    //    var result =
    //        await _paymentService.HandleReturnAsync(
    //            PaymentProvider.VNPay,
    //            parameters,
    //            cancellationToken);

    //    return Ok(result);
    //}

    [AllowAnonymous]
    [HttpGet("vnpay-return")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VnPayReturn(
        CancellationToken cancellationToken)
    {
        var parameters =
            Request.Query.ToDictionary(
                x => x.Key,
                x => x.Value.ToString(),
                StringComparer.Ordinal);

        await _paymentService.HandleCallbackAsync(
            PaymentProvider.VNPay,
            parameters,
            cancellationToken);

        return Ok(new
        {
            message = "VNPay callback processed successfully."
        });
    }

    private Guid GetUserId()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        return parsedUserId;
    }
}
