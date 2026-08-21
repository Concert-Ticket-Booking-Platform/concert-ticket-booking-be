namespace ConcertTicket.Application.Auth.DTOs;

public sealed record AuthResponse(
    Guid UserId,
    string Username,
    string Email,
    string AccessToken);
