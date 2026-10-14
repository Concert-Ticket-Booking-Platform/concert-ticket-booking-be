using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Vouchers.DTOs;
using ConcertTicket.Application.Vouchers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/vouchers")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminVoucherController : ControllerBase
    {
        private readonly IAdminVoucherService _voucherService;

        public AdminVoucherController(IAdminVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        /// <summary>
        /// Create a new voucher.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AdminVoucherDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AdminVoucherDto>> Create(
            [FromBody] CreateVoucherRequest request,
            CancellationToken cancellationToken)
        {
            var voucher = await _voucherService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = voucher.Id },
                voucher);
        }

        /// <summary>
        /// Get all vouchers.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AdminVoucherDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<AdminVoucherDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            var result = await _voucherService.GetAllAsync(
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get a voucher by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AdminVoucherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdminVoucherDto>> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var voucher = await _voucherService.GetByIdAsync(
                id,
                cancellationToken);

            return Ok(voucher);
        }

        /// <summary>
        /// Update voucher details.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AdminVoucherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AdminVoucherDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateVoucherRequest request,
            CancellationToken cancellationToken)
        {
            var voucher = await _voucherService.UpdateAsync(
                id,
                request,
                cancellationToken);

            return Ok(voucher);
        }

        /// <summary>
        /// Update voucher status.
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(AdminVoucherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AdminVoucherDto>> UpdateStatus(
            [FromRoute] Guid id,
            [FromBody] UpdateVoucherStatusRequest request,
            CancellationToken cancellationToken)
        {
            var voucher = await _voucherService.UpdateStatusAsync(
                id,
                request,
                cancellationToken);

            return Ok(voucher);
        }

    }
}
