using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Users.DTOs;
using ConcertTicket.Application.Users.Interfaces;
using ConcertTicket.Application.Users.Mappers;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Users.Services;

public sealed class AdminUserService : IAdminUserService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public AdminUserService(IApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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

    public async Task<AdminUserDto> CreateAsync(
        CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.");
        }

        if (request.Role != RoleType.Customer &&
            request.Role != RoleType.Operator)
        {
            throw new ArgumentException(
                "Only Customer and Operator accounts can be created.");
        }

        var emailExists = await _dbContext.Users
            .AnyAsync(x => x.Email.ToLower() == email, cancellationToken);

        if (emailExists)
            throw new InvalidOperationException("Email already exists.");

        var usernameExists = await _dbContext.Users
            .AnyAsync(
                x => x.Username.ToLower() == username.ToLower(),
                cancellationToken);

        if (usernameExists)
            throw new InvalidOperationException("Username already exists.");

        var roleName = request.Role.ToString();

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(
                x => x.RoleName == roleName,
                cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException(
                $"Role '{roleName}' has not been configured.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            Password = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            RoleId = role.Id
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Nạp Role để mapper có thể trả RoleName.
        user.Role = role;

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
