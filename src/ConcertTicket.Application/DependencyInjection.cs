using ConcertTicket.Application.Concerts.Interfaces;
using ConcertTicket.Application.Concerts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConcertTicket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IConcertService, ConcertService>();

        return services;
    }
}