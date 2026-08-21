using ConcertTicket.Application.Auth.Interfaces;
using ConcertTicket.Application.Auth.Services;
using ConcertTicket.Application.Bookings.Interfaces;
using ConcertTicket.Application.Bookings.Services;
using ConcertTicket.Application.Concerts.Interfaces;
using ConcertTicket.Application.Concerts.Services;
using ConcertTicket.Application.Vouchers.Interfaces;
using ConcertTicket.Application.Vouchers.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConcertTicket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IVoucherService, VoucherService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IVoucherService, VoucherService>();

        return services;
    }
}