using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Infrastructure.Persistence.Seed
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(
            AppDbContext dbContext,
            IPasswordHasher passwordHasher)
        {
            await SeedRolesAsync(dbContext);
            await SeedUsersAsync(dbContext, passwordHasher);
            await SeedConcertAsync(dbContext);
            await SeedVouchersAsync(dbContext);
        }

        private static async Task SeedRolesAsync(
            AppDbContext dbContext)
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

        private static async Task SeedUsersAsync(
            AppDbContext dbContext,
            IPasswordHasher passwordHasher)
        {
            var customerRole = await dbContext.Roles
                .SingleAsync(x => x.RoleName == "Customer");

            var operatorRole = await dbContext.Roles
                .SingleAsync(x => x.RoleName == "Operator");

            var adminRole = await dbContext.Roles
                .SingleAsync(x => x.RoleName == "Admin");

            var now = DateTimeOffset.UtcNow;

            // Admin
            if (!await dbContext.Users.AnyAsync(x => x.Username == "admin"))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@gmail.com",
                    Password = passwordHasher.Hash("12345"),
                    IsActive = true,
                    CreatedAt = now,
                    RoleId = adminRole.Id
                };

                await dbContext.Users.AddAsync(admin);
            }

            // Operator
            if (!await dbContext.Users.AnyAsync(x => x.Username == "operator"))
            {
                var operatorUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "operator",
                    Email = "operator@gmail.com",
                    Password = passwordHasher.Hash("12345"),
                    IsActive = true,
                    CreatedAt = now,
                    RoleId = operatorRole.Id
                };

                await dbContext.Users.AddAsync(operatorUser);
            }

            // Customer
            if (!await dbContext.Users.AnyAsync(x => x.Username == "customer"))
            {
                var customer = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "customer",
                    Email = "customer@gmail.com",
                    Password = passwordHasher.Hash("12345"),
                    IsActive = true,
                    CreatedAt = now,
                    RoleId = customerRole.Id
                };

                await dbContext.Users.AddAsync(customer);
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedConcertAsync(
            AppDbContext dbContext)
        {
            if (await dbContext.Concerts.AnyAsync())
                return;

            var admin = await dbContext.Users
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
                    Price = 1500000m,
                    TotalQuantity = 500,
                    AvailableQuantity = 500,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    Status = TicketCategoryStatus.Active,
                    CreatedAt = now,
                    UpdatedAt = now
                },

                new TicketCategory
                {
                    Id = Guid.NewGuid(),
                    ConcertId = concertId,
                    Name = "Standard",
                    Price = 700000m,
                    TotalQuantity = 2000,
                    AvailableQuantity = 2000,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    Status = TicketCategoryStatus.Active,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await dbContext.TicketCategories.AddRangeAsync(categories);

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedVouchersAsync(
            AppDbContext dbContext)
        {
            if (await dbContext.Vouchers.AnyAsync())
                return;

            var now = DateTimeOffset.UtcNow;

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = "LAUNCH2026",
                Name = "Launch Week 10% Off",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10m,
                MaxDiscountAmount = null,
                UsageLimit = 1000,
                UsedCount = 0,
                StartsAt = now,
                ExpiresAt = now.AddDays(30),
                Status = VoucherStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };

            await dbContext.Vouchers.AddAsync(voucher);

            await dbContext.SaveChangesAsync();
        }
    }
}
