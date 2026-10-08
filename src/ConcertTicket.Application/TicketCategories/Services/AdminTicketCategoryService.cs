using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.TicketCategories.DTOs;
using ConcertTicket.Application.TicketCategories.Interfaces;
using ConcertTicket.Application.TicketCategories.Mappers;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.TicketCategories.Services;

public sealed class AdminTicketCategoryService : IAdminTicketCategoryService
{
    private readonly IApplicationDbContext _dbContext;

    public AdminTicketCategoryService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminTicketCategoryDto> CreateAsync(
        Guid concertId,
        CreateTicketCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var concert = await _dbContext.Concerts
            .FirstOrDefaultAsync(
                x => x.Id == concertId,
                cancellationToken);

        if (concert is null)
        {
            throw new KeyNotFoundException(
                $"Concert '{concertId}' was not found.");
        }

        EnsureConcertCanManageTicketCategories(concert);

        var normalizedName = request.Name.Trim();

        var duplicateExists = await _dbContext.TicketCategories
            .AnyAsync(
                x => x.ConcertId == concertId &&
                     x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Ticket category '{normalizedName}' already exists for this concert.");
        }

        var now = DateTimeOffset.UtcNow;

        var category = new TicketCategory
        {
            Id = Guid.NewGuid(),
            ConcertId = concert.Id,
            Name = normalizedName,
            Price = request.Price,
            TotalQuantity = request.TotalQuantity,
            AvailableQuantity = request.TotalQuantity,
            ReservedQuantity = 0,
            SoldQuantity = 0,
            Status = TicketCategoryStatus.Active,
            CreatedAt = now
        };

        _dbContext.TicketCategories.Add(category);

        await _dbContext.SaveChangesAsync(cancellationToken);

        category.Concert = concert;

        return TicketCategoryMapper.ToAdminDto(category);
    }

    public async Task<AdminTicketCategoryDto?> GetByIdAsync(
        Guid ticketCategoryId,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.TicketCategories
            .AsNoTracking()
            .Include(x => x.Concert)
            .FirstOrDefaultAsync(
                x => x.Id == ticketCategoryId,
                cancellationToken);

        return category is null
            ? null
            : TicketCategoryMapper.ToAdminDto(category);
    }

    public async Task<IReadOnlyList<AdminTicketCategoryDto>>
        GetByConcertIdAsync(
            Guid concertId,
            CancellationToken cancellationToken = default)
    {
        var concertExists = await _dbContext.Concerts
            .AnyAsync(
                x => x.Id == concertId,
                cancellationToken);

        if (!concertExists)
        {
            throw new KeyNotFoundException(
                $"Concert '{concertId}' was not found.");
        }

        var categories = await _dbContext.TicketCategories
            .AsNoTracking()
            .Include(x => x.Concert)
            .Where(x => x.ConcertId == concertId)
            .OrderBy(x => x.Price)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return categories
            .Select(TicketCategoryMapper.ToAdminDto)
            .ToList();
    }

    public async Task<AdminTicketCategoryDto> UpdateAsync(
        Guid ticketCategoryId,
        UpdateTicketCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdateRequest(request);

        var category = await _dbContext.TicketCategories
            .Include(x => x.Concert)
            .FirstOrDefaultAsync(
                x => x.Id == ticketCategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Ticket category '{ticketCategoryId}' was not found.");
        }

        EnsureConcertCanManageTicketCategories(category.Concert);

        var normalizedName = request.Name.Trim();

        var duplicateExists = await _dbContext.TicketCategories
            .AnyAsync(
                x => x.ConcertId == category.ConcertId &&
                     x.Id != category.Id &&
                     x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Ticket category '{normalizedName}' already exists for this concert.");
        }

        var committedQuantity = category.ReservedQuantity + category.SoldQuantity;

        if (request.TotalQuantity < committedQuantity)
        {
            throw new InvalidOperationException(
                "Total quantity cannot be less than reserved quantity plus sold quantity.");
        }

        var newAvailableQuantity =
            request.TotalQuantity - committedQuantity;

        category.Name = normalizedName;
        category.Price = request.Price;
        category.TotalQuantity = request.TotalQuantity;
        category.AvailableQuantity = newAvailableQuantity;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        if (newAvailableQuantity > 0 &&
            category.Status == TicketCategoryStatus.SoldOut)
        {
            category.Status = TicketCategoryStatus.Active;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return TicketCategoryMapper.ToAdminDto(category);
    }

    public async Task<AdminTicketCategoryDto> UpdateStatusAsync(
        Guid ticketCategoryId,
        UpdateTicketCategoryStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Status == TicketCategoryStatus.SoldOut)
        {
            throw new InvalidOperationException(
                "SoldOut status is managed automatically by inventory.");
        }

        var category = await _dbContext.TicketCategories
            .Include(x => x.Concert)
            .FirstOrDefaultAsync(
                x => x.Id == ticketCategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Ticket category '{ticketCategoryId}' was not found.");
        }

        EnsureConcertCanManageTicketCategories(category.Concert);

        if (category.Status == request.Status)
        {
            throw new InvalidOperationException(
                $"Ticket category is already in status '{request.Status}'.");
        }

        if (request.Status == TicketCategoryStatus.Active &&
            category.AvailableQuantity <= 0)
        {
            throw new InvalidOperationException(
                "A ticket category with no available quantity cannot be activated.");
        }

        category.Status = request.Status;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return TicketCategoryMapper.ToAdminDto(category);
    }

    private static void ValidateCreateRequest(
        CreateTicketCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Ticket category name is required.");
        }

        if (request.Price <= 0)
        {
            throw new ArgumentException(
                "Ticket category price must be greater than zero.");
        }

        if (request.TotalQuantity <= 0)
        {
            throw new ArgumentException(
                "Total quantity must be greater than zero.");
        }
    }

    private static void ValidateUpdateRequest(
        UpdateTicketCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Ticket category name is required.");
        }

        if (request.Price <= 0)
        {
            throw new ArgumentException(
                "Ticket category price must be greater than zero.");
        }

        if (request.TotalQuantity <= 0)
        {
            throw new ArgumentException(
                "Total quantity must be greater than zero.");
        }
    }

    private static void EnsureConcertCanManageTicketCategories(
        Concert concert)
    {
        if (concert.Status is
            ConcertStatus.Cancelled or
            ConcertStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Ticket categories cannot be modified for a concert with status '{concert.Status}'.");
        }
    }
}
