using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Users.DTOs;
using ConcertTicket.Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _userService;

    public AdminUsersController(IAdminUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? roleId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            search: search,
            isActive: isActive,
            roleId: roleId,
            cancellationToken: cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateAdminUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(actorUserIdValue, out var actorUserId))
        {
            return Unauthorized();
        }

        var result = await _userService.UpdateStatusAsync(
            userId: id,
            isActive: request.IsActive,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return Ok(result);
    }
}
