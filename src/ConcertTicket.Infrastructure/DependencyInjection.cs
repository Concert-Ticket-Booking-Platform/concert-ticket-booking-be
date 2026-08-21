using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Infrastructure.Persistence;
using ConcertTicket.Infrastructure.Persistence.Repositories;
using ConcertTicket.Infrastructure.Security;
using ConcertTicket.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConcertTicket.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            services.AddScoped<IUnitOfWork>(provider =>
              provider.GetRequiredService<AppDbContext>());

            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IBookingCodeGenerator, BookingCodeGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
