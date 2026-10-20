using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Users.DTOs;

namespace ConcertTicket.Application.Users.Interfaces;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        bool? isActive = null,
        Guid? roleId = null,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto> UpdateStatusAsync(
        Guid userId,
        bool isActive,
        Guid actorUserId,
        CancellationToken cancellationToken = default);
}
