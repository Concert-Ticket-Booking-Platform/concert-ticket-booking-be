using ConcertTicket.Application.Users.DTOs;
using ConcertTicket.Domain.Entities;

namespace ConcertTicket.Application.Users.Mappers;

public static class AdminUserMapper
{
    public static AdminUserDto ToDto(User user)
    {
        return new AdminUserDto(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            Phone: user.Phone,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AvatarUrl: user.AvatarUrl,
            IsActive: user.IsActive,
            RoleId: user.RoleId,
            RoleName: user.Role.RoleName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }
}
