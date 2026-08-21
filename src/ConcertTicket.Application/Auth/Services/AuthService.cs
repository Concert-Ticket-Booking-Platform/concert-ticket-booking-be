using ConcertTicket.Application.Auth.DTOs;
using ConcertTicket.Application.Auth.Interfaces;
using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var usernameExists =
            await _dbContext.Users.AnyAsync(
                x => x.Username == request.Username,
                cancellationToken);

        if (usernameExists)
            throw new InvalidOperationException(
                "Username already exists.");

        var emailExists =
            await _dbContext.Users.AnyAsync(
                x => x.Email == request.Email,
                cancellationToken);

        if (emailExists)
            throw new InvalidOperationException(
                "Email already exists.");

        var customerRole =
            await _dbContext.Roles.FirstOrDefaultAsync(
                x => x.RoleName == "Customer",
                cancellationToken);

        if (customerRole is null)
            throw new InvalidOperationException(
                "Customer role is not configured.");

        var now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),

            Username = request.Username,

            Email = request.Email,

            Password =
                _passwordHasher.Hash(request.Password),

            Phone = request.Phone,

            FirstName = request.FirstName,

            LastName = request.LastName,

            IsActive = true,

            CreatedAt = now,

            RoleId = customerRole.Id
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var token =
            _jwtTokenService.GenerateToken(
                user.Id,
                user.Username,
                customerRole.RoleName);

        return new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            token);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user =
            await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(
                    x =>
                        x.Username == request.Username,
                    cancellationToken);

        if (user is null ||
            !_passwordHasher.Verify(
                request.Password,
                user.Password))
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        var token =
            _jwtTokenService.GenerateToken(
                user.Id,
                user.Username,
                user.Role.RoleName);

        return new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            token);
    }
}
