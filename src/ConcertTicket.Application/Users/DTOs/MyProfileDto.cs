namespace ConcertTicket.Application.Users.DTOs;

public sealed record MyProfileDto(
    Guid Id,
    string Email,
    string Username,
    string? Phone,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool IsActive,
    string RoleName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
