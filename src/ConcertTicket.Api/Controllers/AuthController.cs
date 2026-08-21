using ConcertTicket.Application.Auth.DTOs;
using ConcertTicket.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _authService.RegisterAsync(
                request,
                cancellationToken);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _authService.LoginAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
}