using ConcertTicket.Application.Users.DTOs;
using ConcertTicket.Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/users/me")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IMyAccountService _myAccountService;

    public UsersController(IMyAccountService myAccountService)
    {
        _myAccountService = myAccountService;
    }

    [HttpGet]
    public async Task<ActionResult<MyProfileDto>> GetMyProfile(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _myAccountService.GetMyProfileAsync(
            userId,
            cancellationToken);

        return Ok(profile);
    }

    [HttpPatch]
    public async Task<ActionResult<MyProfileDto>> UpdateMyProfile(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _myAccountService.UpdateMyProfileAsync(
            userId,
            request,
            cancellationToken);

        return Ok(profile);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeMyPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        await _myAccountService.ChangeMyPasswordAsync(
            userId,
            request,
            cancellationToken);

        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }
}
