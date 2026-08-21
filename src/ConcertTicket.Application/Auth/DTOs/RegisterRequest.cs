namespace ConcertTicket.Application.Auth.DTOs;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? Phone,
    string? FirstName,
    string? LastName);