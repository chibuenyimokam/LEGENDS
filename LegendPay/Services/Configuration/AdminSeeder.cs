using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Configuration
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
        {
            var email = configuration["AdminSeed:Email"];
            var password = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await context.AdminAccounts.AnyAsync(a => a.Email == email))
                return;

            context.AdminAccounts.Add(new AdminAccount
            {
                FirstName = configuration["AdminSeed:FirstName"] ?? "Admin",
                LastName = configuration["AdminSeed:LastName"] ?? "User",
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "SuperAdmin",
                IsActive = true
            });

            await context.SaveChangesAsync();
        }
    }
}
