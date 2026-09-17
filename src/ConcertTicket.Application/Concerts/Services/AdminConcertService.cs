using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Concerts.DTOs;
using ConcertTicket.Application.Concerts.Interfaces;
using ConcertTicket.Application.Concerts.Mappers;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Concerts.Services;

public sealed class AdminConcertService : IAdminConcertService
{
    private readonly IApplicationDbContext _dbContext;

    public AdminConcertService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminConcertDto> CreateAsync(
        CreateConcertRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var creator = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == currentUserId && x.IsActive,
                cancellationToken);

        if (creator is null)
        {
            throw new InvalidOperationException(
                "Current user does not exist or is inactive.");
        }

        var concert = new Concert
        {
            Id = Guid.NewGuid(),
            ConcertName = request.ConcertName.Trim(),
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            Venue = request.Venue.Trim(),
            EventDate = request.EventDate,
            Status = ConcertStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = currentUserId
        };

        _dbContext.Concerts.Add(concert);

        await _dbContext.SaveChangesAsync(cancellationToken);

        concert.Creator = creator;

        return ConcertMapper.ToAdminDto(concert);
    }

    public async Task<AdminConcertDto?> GetByIdAsync(
        Guid concertId,
        CancellationToken cancellationToken = default)
    {
        var concert = await _dbContext.Concerts
            .AsNoTracking()
            .Include(x => x.Creator)
            .FirstOrDefaultAsync(
                x => x.Id == concertId,
                cancellationToken);

        return concert is null
            ? null
            : ConcertMapper.ToAdminDto(concert);
    }

    public async Task<IReadOnlyList<AdminConcertDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var concerts = await _dbContext.Concerts
            .AsNoTracking()
            .Include(x => x.Creator)
            .OrderByDescending(x => x.EventDate)
            .ToListAsync(cancellationToken);

        return concerts
            .Select(ConcertMapper.ToAdminDto)
            .ToList();
    }

    public async Task<AdminConcertDto> UpdateAsync(
        Guid concertId,
        UpdateConcertRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);

        var concert = await _dbContext.Concerts
            .Include(x => x.Creator)
            .FirstOrDefaultAsync(
                x => x.Id == concertId,
                cancellationToken);

        if (concert is null)
        {
            throw new KeyNotFoundException(
                $"Concert '{concertId}' was not found.");
        }

        EnsureConcertCanBeUpdated(concert);

        concert.ConcertName = request.ConcertName.Trim();
        concert.Description = request.Description?.Trim();
        concert.ImageUrl = request.ImageUrl?.Trim();
        concert.Venue = request.Venue.Trim();
        concert.EventDate = request.EventDate;
        concert.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ConcertMapper.ToAdminDto(concert);
    }

    public async Task<AdminConcertDto> UpdateStatusAsync(
        Guid concertId,
        UpdateConcertStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var concert = await _dbContext.Concerts
            .Include(x => x.Creator)
            .FirstOrDefaultAsync(
                x => x.Id == concertId,
                cancellationToken);

        if (concert is null)
        {
            throw new KeyNotFoundException(
                $"Concert '{concertId}' was not found.");
        }

        ValidateStatusTransition(
            concert.Status,
            request.Status,
            concert.EventDate);

        concert.Status = request.Status;
        concert.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ConcertMapper.ToAdminDto(concert);
    }

    private static void ValidateCreateRequest(
        CreateConcertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ConcertName))
        {
            throw new ArgumentException(
                "Concert name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Venue))
        {
            throw new ArgumentException(
                "Venue is required.");
        }

        if (request.EventDate <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Event date must be in the future.");
        }
    }

    private static void ValidateUpdateRequest(
        UpdateConcertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ConcertName))
        {
            throw new ArgumentException(
                "Concert name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Venue))
        {
            throw new ArgumentException(
                "Venue is required.");
        }

        if (request.EventDate <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Event date must be in the future.");
        }
    }

    private static void EnsureConcertCanBeUpdated(
        Concert concert)
    {
        if (concert.Status is
            ConcertStatus.Cancelled or
            ConcertStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Concert with status '{concert.Status}' cannot be updated.");
        }
    }

    private static void ValidateStatusTransition(
        ConcertStatus currentStatus,
        ConcertStatus newStatus,
        DateTimeOffset eventDate)
    {
        if (currentStatus == newStatus)
        {
            throw new InvalidOperationException(
                $"Concert is already in status '{currentStatus}'.");
        }

        var isValid = currentStatus switch
        {
            ConcertStatus.Draft =>
                newStatus == ConcertStatus.Published,

            ConcertStatus.Published =>
                newStatus == ConcertStatus.Cancelled ||
                newStatus == ConcertStatus.Completed,

            ConcertStatus.Cancelled =>
                false,

            ConcertStatus.Completed =>
                false,

            _ => false
        };

        if (!isValid)
        {
            throw new InvalidOperationException(
                $"Invalid concert status transition: " +
                $"{currentStatus} -> {newStatus}.");
        }

        if (newStatus == ConcertStatus.Completed &&
            eventDate > DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException(
                "Concert cannot be completed before its event date.");
        }
    }
}
