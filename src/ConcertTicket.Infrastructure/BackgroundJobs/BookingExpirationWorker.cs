using ConcertTicket.Application.Bookings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConcertTicket.Infrastructure.BackgroundJobs;

/// <summary>
/// A background worker that periodically checks for expired bookings and expires them.
/// </summary>
public sealed class BookingExpirationWorker
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpirationWorker> _logger;

    public BookingExpirationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingExpirationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var expirationService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IBookingExpirationService>();

                var count =
                    await expirationService
                        .ExpireBookingsAsync(
                            stoppingToken);

                if (count > 0)
                {
                    _logger.LogInformation(
                        "Expired {Count} bookings.",
                        count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while expiring bookings.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}
