using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Payments.DTOs;
using ConcertTicket.Application.Payments.Interfaces;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Payments.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IEnumerable<IPaymentProvider> _providers;

    public PaymentService(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IInventoryRepository inventoryRepository,
        IEnumerable<IPaymentProvider> providers)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _providers = providers;
    }

    public async Task<CreatePaymentResponse> CreateAsync(
        Guid userId,
        CreatePaymentRequest request,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings
            .Include(x => x.BookingItems)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.BookingId &&
                    x.UserId == userId,
                cancellationToken);

        if (booking is null)
            throw new InvalidOperationException(
                "Booking was not found.");

        if (booking.Status != BookingStatus.WaitingForPayment)
            throw new InvalidOperationException(
                "Booking is not waiting for payment.");

        if (booking.ExpiresAt.HasValue &&
            booking.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException(
                "Booking has expired.");
        }

        var provider = _providers.FirstOrDefault(
            x => x.Provider == request.Provider);

        if (provider is null)
            throw new InvalidOperationException(
                $"Payment provider '{request.Provider}' is not supported.");

        var now = DateTimeOffset.UtcNow;

        var orderCode = GenerateOrderCode();

        //var payment = new PaymentTransaction
        //{
        //    Id = Guid.NewGuid(),
        //    BookingId = booking.Id,
        //    Amount = booking.FinalAmount,
        //    Status = PaymentStatus.Pending,
        //    OrderCode = orderCode,
        //    Provider = request.Provider,
        //    CreatedAt = now,
        //    UpdatedAt = now,
        //    ExpiredAt = booking.ExpiresAt
        //};

        var payment = await _dbContext.PaymentTransactions
            .Where(x =>
                x.BookingId == booking.Id &&
                x.Provider == request.Provider &&
                x.Status == PaymentStatus.Pending &&
                (x.ExpiredAt == null || x.ExpiredAt > now))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            payment = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = booking.FinalAmount,
                Status = PaymentStatus.Pending,
                OrderCode = GenerateOrderCode(),
                Provider = request.Provider,
                CreatedAt = now,
                UpdatedAt = now,
                ExpiredAt = booking.ExpiresAt
            };

            _dbContext.PaymentTransactions.Add(payment);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var paymentRequest = new PaymentCreationRequest(
            payment.Id,
            payment.OrderCode,
            payment.Amount,
            $"Payment for booking {booking.BookingCode}",
            GetReturnUrl(request.Provider),
            ipAddress,
            booking.ExpiresAt ?? now.AddMinutes(15));

        var result = await provider.CreatePaymentAsync(
            paymentRequest,
            cancellationToken);

        if (!result.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw new InvalidOperationException(
                result.ErrorMessage ??
                "Unable to create payment.");
        }

        //payment.TransactionReference = payment.Id.ToString();
        payment.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new CreatePaymentResponse(
            payment.Id,
            booking.Id,
            request.Provider.ToString(),
            payment.Amount,
            result.PaymentUrl,
            payment.ExpiredAt);
    }

    public async Task HandleCallbackAsync(
        PaymentProvider provider,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var paymentProvider = _providers.FirstOrDefault(
            x => x.Provider == provider);

        if (paymentProvider is null)
            throw new InvalidOperationException(
                "Payment provider is not supported.");

        var callback =
            await paymentProvider.ProcessCallbackAsync(
                parameters,
                cancellationToken);

        if (!callback.IsValid)
            throw new InvalidOperationException(
                "Invalid payment callback.");

        if (!long.TryParse(
                GetOrderCode(provider, parameters),
                out var orderCode))
        {
            throw new InvalidOperationException(
                "Invalid order code.");
        }

        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var payment = await _dbContext.PaymentTransactions
                .Include(x => x.Booking)
                .ThenInclude(x => x.BookingItems)
                .FirstOrDefaultAsync(
                    x =>
                        x.OrderCode == orderCode &&
                        x.Provider == provider,
                    cancellationToken);

            if (payment is null)
                throw new InvalidOperationException(
                    "Payment transaction was not found.");

            // Idempotent callback.
            if (payment.Status == PaymentStatus.Paid)
            {
                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return;
            }

            // Provider callback must contain amount.
            if (!callback.Amount.HasValue)
            {
                throw new InvalidOperationException(
                    "Payment callback amount is missing.");
            }

            // Amount must match our payment transaction.
            if (payment.Amount != callback.Amount.Value)
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTimeOffset.UtcNow;
                payment.ResponsePayload = callback.ResponsePayload;

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return;
            }

            if (!callback.IsSuccess)
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTimeOffset.UtcNow;
                payment.ResponsePayload =
                    callback.ResponsePayload;

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return;
            }

            var booking = payment.Booking;

            if (booking.Status != BookingStatus.WaitingForPayment)
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return;
            }

            if (booking.ExpiresAt.HasValue &&
                booking.ExpiresAt.Value <= DateTimeOffset.UtcNow)
            {
                payment.Status = PaymentStatus.Expired;
                payment.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return;
            }

            foreach (var item in booking.BookingItems)
            {
                var confirmed =
                    await _inventoryRepository.ConfirmAsync(
                        item.TicketCategoryId,
                        item.Quantity,
                        cancellationToken);

                if (confirmed != 1)
                {
                    throw new InvalidOperationException(
                        $"Unable to confirm inventory for booking {booking.Id}.");
                }
            }

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTimeOffset.UtcNow;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            payment.TransactionReference = callback.TransactionReference;
            payment.ResponsePayload = callback.ResponsePayload;

            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTimeOffset.UtcNow;
            booking.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }


    private static long GenerateOrderCode()
    {
        var timestamp =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var random =
            Random.Shared.Next(1000, 9999);

        return timestamp * 10_000L + random;
    }

    private static string GetReturnUrl(
        PaymentProvider provider)
    {
        return provider switch
        {
            PaymentProvider.VNPay =>
                "https://localhost:8080/api/v1/payments/vnpay-return",

            PaymentProvider.MoMo =>
                "https://localhost:8080/api/v1/payments/momo-return",

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetOrderCode(
        PaymentProvider provider,
        IDictionary<string, string> parameters)
    {
        return provider switch
        {
            PaymentProvider.VNPay =>
                parameters["vnp_TxnRef"],

            PaymentProvider.MoMo =>
                parameters["orderId"],

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
