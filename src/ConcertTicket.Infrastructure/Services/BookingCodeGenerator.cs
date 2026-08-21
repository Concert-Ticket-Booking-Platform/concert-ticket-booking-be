using ConcertTicket.Application.Common.Interfaces;

namespace ConcertTicket.Infrastructure.Services;

public sealed class BookingCodeGenerator
    : IBookingCodeGenerator
{
    public string Generate()
    {
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
            .ToUpperInvariant();
    }
}
