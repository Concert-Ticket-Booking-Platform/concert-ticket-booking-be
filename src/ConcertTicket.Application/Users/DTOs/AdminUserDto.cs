namespace ConcertTicket.Application.Users.DTOs;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string Username,
    string? Phone,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool IsActive,
    Guid RoleId,
    string RoleName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
