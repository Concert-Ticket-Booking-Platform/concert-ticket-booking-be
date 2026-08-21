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

            var category = await _dbContext.TicketCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.TicketCategoryId &&
                        x.ConcertId == request.ConcertId &&
                        x.Status == TicketCategoryStatus.Active,
                    cancellationToken);

            if (category is null)
                throw new InvalidOperationException(
                    "Ticket category is not available.");

            var subtotal =
                category.Price * request.Quantity;

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
                        subtotal);
            }

            var reserved =
                await _inventoryRepository.ReserveAsync(
                    request.TicketCategoryId,
                    request.Quantity,
                    cancellationToken);

            if (reserved != 1)
                throw new InvalidOperationException(
                    "Not enough tickets available.");

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

                TotalAmount = subtotal,

                DiscountAmount = discountAmount,

                FinalAmount =
                    subtotal - discountAmount,

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

            var bookingItem = new BookingItem
            {
                Id = Guid.NewGuid(),

                Quantity = request.Quantity,

                UnitPrice = category.Price,

                Subtotal = subtotal,

                CreatedAt = now,

                TicketCategoryId =
                    request.TicketCategoryId,

                BookingId = booking.Id
            };

            booking.BookingItems.Add(bookingItem);

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

    private static void ValidateRequest(
        CreateBookingRequest request,
        string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException(
                "Idempotency-Key is required.");

        if (request.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (request.Quantity > 10)
            throw new ArgumentException(
                "Maximum 10 tickets per booking.");
    }

    private static CreateBookingResponse MapResponse(
        Booking booking)
    {
        return new CreateBookingResponse(
            booking.Id,
            booking.BookingCode,
            booking.Status,
            booking.TotalAmount,
            booking.DiscountAmount,
            booking.FinalAmount,
            booking.ExpiresAt);
    }
}