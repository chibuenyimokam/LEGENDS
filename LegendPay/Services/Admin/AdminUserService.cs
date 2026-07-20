using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _context;

        public AdminUserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminUserRegistryViewModel> GetUserRegistryAsync(string? search, string? status, decimal? minBalance, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;

            var totalUsers = await _context.UserAccounts.CountAsync();
            var verifiedUsers = await _context.UserAccounts.CountAsync(u => u.IsEmailVerified);

            var query = _context.UserAccounts.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u =>
                    u.FirstName.Contains(s) ||
                    u.LastName.Contains(s) ||
                    u.Email.Contains(s) ||
                    u.PhoneNumber.Contains(s));
            }

            query = status switch
            {
                "verified" => query.Where(u => u.IsEmailVerified),
                "unverified" => query.Where(u => !u.IsEmailVerified),
                _ => query
            };

            if (minBalance.HasValue)
                query = query.Where(u => u.Wallet != null && u.Wallet.Balance >= minBalance.Value);

            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserRow
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Balance = u.Wallet != null ? u.Wallet.Balance : 0m,
                    IsVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return new AdminUserRegistryViewModel
            {
                TotalUsers = totalUsers,
                VerifiedUsers = verifiedUsers,
                UnverifiedUsers = totalUsers - verifiedUsers,
                Users = users,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                Search = search,
                Status = status,
                MinBalance = minBalance
            };
        }
    }
}
