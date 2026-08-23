using ConcertTicket.Api.Models;
using ConcertTicket.Application.Vouchers.DTOs;
using ConcertTicket.Application.Vouchers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/vouchers")]
public sealed class VouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _voucherService.GetAllVouchersAsync(cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<VoucherDto>>
        {
            Success = true,
            Data = result
        });
    }
}
