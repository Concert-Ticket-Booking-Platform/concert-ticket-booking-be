using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Users.DTOs;
using ConcertTicket.Application.Users.Interfaces;
using ConcertTicket.Application.Users.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Users.Services;

public sealed class AdminUserService : IAdminUserService
{
    private readonly IApplicationDbContext _dbContext;

    public AdminUserService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminUserDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        bool? isActive = null,
        Guid? roleId = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentException(
                "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException(
                "PageSize must be between 1 and 100.");
        }

        var query = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();

            query = query.Where(user =>
                user.Username.ToLower().Contains(keyword) ||
                user.Email.ToLower().Contains(keyword) ||
                (user.FirstName != null &&
                 user.FirstName.ToLower().Contains(keyword)) ||
                (user.LastName != null &&
                 user.LastName.ToLower().Contains(keyword)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(
                user => user.IsActive == isActive.Value);
        }

        if (roleId.HasValue)
        {
            query = query.Where(
                user => user.RoleId == roleId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Include(user => user.Role)
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = users
            .Select(AdminUserMapper.ToDto)
            .ToList();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        return new PagedResult<AdminUserDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        );
    }

    public async Task<AdminUserDto> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"User '{userId}' was not found.");
        }

        return AdminUserMapper.ToDto(user);
    }

    public async Task<AdminUserDto> UpdateStatusAsync(
        Guid userId,
        bool isActive,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (userId == actorUserId && !isActive)
        {
            throw new InvalidOperationException(
                "You cannot deactivate your own account.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"User '{userId}' was not found.");
        }

        if (user.IsActive == isActive)
        {
            return AdminUserMapper.ToDto(user);
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AdminUserMapper.ToDto(user);
    }
}
