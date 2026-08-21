using ConcertTicket.Application.Auth.DTOs;

namespace ConcertTicket.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}