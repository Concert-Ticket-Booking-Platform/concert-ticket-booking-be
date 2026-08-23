using ConcertTicket.Application.Bookings.DTOs;
using ConcertTicket.Application.Bookings.Interfaces;
using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Vouchers.Interfaces;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Bookings.Services;

public sealed class BookingService : IBookingService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IVoucherService _voucherService;
    private readonly IBookingCodeGenerator _bookingCodeGenerator;

    public BookingService(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IInventoryRepository inventoryRepository,
        IVoucherService voucherService,
        IBookingCodeGenerator bookingCodeGenerator)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _voucherService = voucherService;
        _bookingCodeGenerator = bookingCodeGenerator;
    }

    public async Task<CreateBookingResponse> CreateAsync(
        Guid userId,
        string idempotencyKey,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request, idempotencyKey);

        var existingBooking = await _dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.IdempotencyKey == idempotencyKey,
                cancellationToken);

        if (existingBooking is not null)
        {
            return MapResponse(existingBooking);
        }

        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var concert = await _dbContext.Concerts
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.ConcertId &&
                        x.Status == ConcertStatus.Published,
                    cancellationToken);

            if (concert is null)
                throw new InvalidOperationException(
                    "Concert is not available.");

            var categoryIds = request.Items
            .Select(x => x.TicketCategoryId)
            .Distinct()
            .ToList();

            var categories = await _dbContext.TicketCategories
            .AsNoTracking()
            .Where(x =>
                categoryIds.Contains(x.Id) &&
                x.ConcertId == request.ConcertId &&
                x.Status == TicketCategoryStatus.Active)
            .ToListAsync(cancellationToken);

            if (categories.Count != categoryIds.Count)
            {
                throw new InvalidOperationException(
                    "One or more ticket categories are not available.");
            }

            var categoryDictionary = categories
                .ToDictionary(x => x.Id);

            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var category =
                    categoryDictionary[item.TicketCategoryId];

                totalAmount +=
                    category.Price * item.Quantity;
            }

            var discountAmount = 0m;
            Voucher? voucher = null;

            if (!string.IsNullOrWhiteSpace(
                request.VoucherCode))
            {
                voucher = await _voucherService
                    .GetValidVoucherAsync(
                        request.VoucherCode,
                        cancellationToken);

                if (voucher is null)
                    throw new InvalidOperationException(
                        "Voucher is invalid or unavailable.");

                discountAmount =
                    _voucherService.CalculateDiscount(
                        voucher,
                        totalAmount);
            }

            // Reserve inventory for every ticket category.
            foreach (var item in request.Items)
            {
                var reserved =
                    await _inventoryRepository.ReserveAsync(
                        item.TicketCategoryId,
                        item.Quantity,
                        cancellationToken);

                if (reserved != 1)
                {
                    throw new InvalidOperationException(
                        $"Not enough tickets available for category " +
                        $"{item.TicketCategoryId}.");
                }
            }

            // Consume voucher atomically with booking creation.
            if (voucher is not null)
            {
                var consumed =
                    await _dbContext.TryConsumeVoucherAsync(
                        voucher.Id,
                        cancellationToken);

                if (consumed != 1)
                {
                    throw new InvalidOperationException(
                        "Voucher is no longer available.");
                }
            }


            var now = DateTimeOffset.UtcNow;

            var booking = new Booking
            {
                Id = Guid.NewGuid(),

                BookingCode =
                    _bookingCodeGenerator.Generate(),

                TotalAmount = totalAmount,

                DiscountAmount = discountAmount,

                FinalAmount =
                    totalAmount - discountAmount,

                Status =
                    BookingStatus.WaitingForPayment,

                ExpiresAt =
                    now.AddMinutes(15),

                CreatedAt = now,

                UpdatedAt = now,

                IdempotencyKey = idempotencyKey,

                UserId = userId,

                ConcertId = request.ConcertId
            };

            foreach (var item in request.Items)
            {
                var category =
                    categoryDictionary[item.TicketCategoryId];

                var bookingItem = new BookingItem
                {
                    Id = Guid.NewGuid(),

                    Quantity = item.Quantity,

                    UnitPrice = category.Price,

                    Subtotal =
                        category.Price * item.Quantity,

                    CreatedAt = now,

                    TicketCategoryId =
                        item.TicketCategoryId,

                    BookingId = booking.Id
                };

                booking.BookingItems.Add(
                    bookingItem);
            }

            if (voucher is not null)
            {
                var voucherUsage = new VoucherUsage
                {
                    Id = Guid.NewGuid(),

                    UsedAt = now,

                    UserId = userId,

                    BookingId = booking.Id,

                    VoucherId = voucher.Id
                };

                booking.VoucherUsages.Add(
                    voucherUsage);
            }

            _dbContext.Bookings.Add(booking);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return MapResponse(booking);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }

    public async Task<bool> CancelAsync(
    Guid bookingId,
    Guid userId,
    CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var booking = await _dbContext.Bookings
                .Include(x => x.BookingItems)
                .Include(x => x.VoucherUsages)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == bookingId &&
                        x.UserId == userId,
                    cancellationToken);

            if (booking is null)
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                return false;
            }

            if (booking.Status != BookingStatus.WaitingForPayment)
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                throw new InvalidOperationException(
                    "Only bookings waiting for payment can be cancelled.");
            }

            var now = DateTimeOffset.UtcNow;

            foreach (var item in booking.BookingItems)
            {
                var released =
                    await _inventoryRepository.ReleaseAsync(
                        item.TicketCategoryId,
                        item.Quantity,
                        cancellationToken);

                if (released != 1)
                {
                    throw new InvalidOperationException(
                        $"Failed to release inventory for booking {booking.Id}.");
                }
            }

            foreach (var usage in booking.VoucherUsages)
            {
                var released =
                    await _dbContext.ReleaseVoucherUsageAsync(
                        usage.VoucherId,
                        cancellationToken);

                if (released != 1)
                {
                    throw new InvalidOperationException(
                        $"Failed to release voucher usage for booking {booking.Id}.");
                }
            }

            _dbContext.VoucherUsages.RemoveRange(
                booking.VoucherUsages);

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = now;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }

    public async Task<BookingDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings
            .Include(x => x.Concert)
            .Include(x => x.BookingItems)
            .ThenInclude(x => x.TicketCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserId == userId,
                cancellationToken);

        if (booking is null)
        {
            return null;
        }

        return MapToDto(booking);
    }

    public async Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var bookings = await _dbContext.Bookings
            .Include(x => x.Concert)
            .Include(x => x.BookingItems)
            .ThenInclude(x => x.TicketCategory)
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return bookings.Select(MapToDto).ToList();
    }


    private static void ValidateRequest(
        CreateBookingRequest request,
        string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException(
                "Idempotency-Key is required.");

        if (request.ConcertId == Guid.Empty)
        {
            throw new ArgumentException(
                "ConcertId is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one ticket category is required.");
        }

        var duplicateCategory =
        request.Items
            .GroupBy(x => x.TicketCategoryId)
            .Any(x => x.Count() > 1);

        if (duplicateCategory)
        {
            throw new ArgumentException(
                "A ticket category cannot appear more than once.");
        }

        var totalQuantity =
        request.Items.Sum(x => x.Quantity);

        if (totalQuantity <= 0)
        {
            throw new ArgumentException(
                "Total ticket quantity must be greater than zero.");
        }

        if (totalQuantity > 10)
        {
            throw new ArgumentException(
                "Maximum 10 tickets per booking.");
        }

        if (request.Items.Any(x =>
        x.TicketCategoryId == Guid.Empty))
        {
            throw new ArgumentException(
                "TicketCategoryId is required.");
        }

        if (request.Items.Any(x =>
            x.Quantity <= 0))
        {
            throw new ArgumentException(
                "Ticket quantity must be greater than zero.");
        }
    }

    private static BookingDto MapToDto(Booking booking)
    {
        var items = booking.BookingItems.Select(x => new BookingItemDto(
            x.Id,
            x.Quantity,
            x.UnitPrice,
            x.Subtotal,
            x.TicketCategoryId,
            x.TicketCategory?.Name ?? string.Empty
        )).ToList();

        return new BookingDto(
            booking.Id,
            booking.BookingCode,
            booking.TotalAmount,
            booking.DiscountAmount,
            booking.FinalAmount,
            booking.Status.ToString(),
            booking.ExpiresAt,
            booking.CompletedAt,
            booking.CreatedAt,
            booking.ConcertId,
            booking.Concert?.ConcertName ?? string.Empty,
            items
        );
    }

    private static CreateBookingResponse MapResponse(
        Booking booking)
    {
        return new CreateBookingResponse(
            booking.Id,
            booking.BookingCode,
            booking.Status.ToString(),
            booking.TotalAmount,
            booking.DiscountAmount,
            booking.FinalAmount,
            booking.ExpiresAt);
    }
}