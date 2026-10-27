using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Users.DTOs;
using ConcertTicket.Application.Users.Interfaces;
using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Users.Services;

public sealed class MyAccountService : IMyAccountService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public MyAccountService(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<MyProfileDto> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);

        return ToDto(user);
    }

    public async Task<MyProfileDto> UpdateMyProfileAsync(
        Guid userId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserAsync(userId, cancellationToken);

        var username = request.Username.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.");
        }

        var usernameExists = await _dbContext.Users
            .AnyAsync(
                x => x.Username == username && x.Id != userId,
                cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "Username is already in use.");
        }

        user.Username = username;
        user.Phone = NormalizeOptional(request.Phone);
        user.FirstName = NormalizeOptional(request.FirstName);
        user.LastName = NormalizeOptional(request.LastName);
        user.AvatarUrl = NormalizeOptional(request.AvatarUrl);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task ChangeMyPasswordAsync(
        Guid userId,
        ChangeMyPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            throw new ArgumentException(
                "Current password is required.");
        }

        if(string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException(
                "New password is required.");
        }

        //if (string.IsNullOrWhiteSpace(request.NewPassword)
        //    || request.NewPassword.Length < 8
        //    || request.NewPassword.Length > 128)
        //{
        //    throw new ArgumentException(
        //        "New password must contain between 8 and 128 characters.");
        //}

        var user = await GetUserAsync(userId, cancellationToken);

        if (!_passwordHasher.Verify(
                request.CurrentPassword,
                user.Password))
        {
            throw new ArgumentException(
                "Current password is incorrect.");
        }

        if (_passwordHasher.Verify(
                request.NewPassword,
                user.Password))
        {
            throw new ArgumentException(
                "New password must be different from the current password.");
        }

        user.Password = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.Role)
            .SingleOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return user;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static MyProfileDto ToDto(User user)
    {
        return new MyProfileDto(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            Phone: user.Phone,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AvatarUrl: user.AvatarUrl,
            IsActive: user.IsActive,
            RoleName: user.Role.RoleName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }
}
