using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Infrastructure.Persistence.Seed
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext dbContext)
        {
            await SeedRolesAsync(dbContext);

            await SeedUsersAsync(dbContext);

            await SeedConcertAsync(dbContext);

            await SeedVouchersAsync(dbContext);
        }

        private static async Task SeedRolesAsync(AppDbContext dbContext)
        {
            if (await dbContext.Roles.AnyAsync())
                return;

            var roles = new[]
            {
            new Role
            {
                Id = Guid.NewGuid(),
                RoleName = "Customer"
            },
            new Role
            {
                Id = Guid.NewGuid(),
                RoleName = "Operator"
            },
            new Role
            {
                Id = Guid.NewGuid(),
                RoleName = "Admin"
            }
        };

            await dbContext.Roles.AddRangeAsync(roles);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(AppDbContext dbContext)
        {
            if (await dbContext.Users.AnyAsync())
                return;

            var operatorRole = await dbContext.Roles
                .FirstOrDefaultAsync(x => x.RoleName == "Operator");

            var adminRole = await dbContext.Roles
                .FirstOrDefaultAsync(x => x.RoleName == "Admin");

            if (operatorRole is null || adminRole is null)
            {
                throw new InvalidOperationException(
                    "Required roles were not found.");
            }

            var now = DateTimeOffset.UtcNow;

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@gmail.com",
                Username = "admin",
                Password = "12345",
                IsActive = true,
                CreatedAt = now,
                RoleId = adminRole.Id
            };

            var operation = new User
            {
                Id = Guid.NewGuid(),
                Email = "operator@gmail.com",
                Username = "operator",
                Password = "12345",
                IsActive = true,
                CreatedAt = now,
                RoleId = operatorRole.Id
            };

            await dbContext.Users.AddRangeAsync(admin, operation);

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedConcertAsync(AppDbContext dbContext)
        {
            if (await dbContext.Concerts.AnyAsync())
                return;

            var admin = await dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == "admin");

            if (admin is null)
            {
                throw new InvalidOperationException(
                    "Seed admin user was not found.");
            }

            var concertId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var concert = new Concert
            {
                Id = concertId,
                ConcertName = "Summer Music Festival 2026",
                Description = "Flash Sale Demo Concert",
                Venue = "National Convention Center",
                EventDate = now.AddDays(30),
                Status = ConcertStatus.Published,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = admin.Id
            };

            await dbContext.Concerts.AddAsync(concert);

            var categories = new[]
            {
            new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = concertId,
                Name = "VIP",
                Price = 1500000,
                TotalQuantity = 500,
                AvailableQuantity = 500
            },
            new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = concertId,
                Name = "Standard",
                Price = 700000,
                TotalQuantity = 2000,
                AvailableQuantity = 2000
            }
            };

            await dbContext.TicketCategories.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedVouchersAsync(AppDbContext dbContext)
        {
            if (await dbContext.Vouchers.AnyAsync())
                return;

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = "LAUNCH2026",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                UsageLimit = 1000,
                UsedCount = 0,
                StartsAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                Status = VoucherStatus.Active
            };

            await dbContext.Vouchers.AddAsync(voucher);
        }
    }
}
