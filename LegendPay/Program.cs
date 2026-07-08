using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data;
using LegendPay.Services.Account;
using LegendPay.Services.Transaction;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using LegendPay.Interfaces.Admin;
using LegendPay.Services.Admin;
using Microsoft.AspNetCore.SignalR;
using LegendPay.Hubs;
using LegendPay.Interfaces;
using LegendPay.Services;

namespace LegendPay
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("default")));

            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddSingleton<WalletTokenCache>();

            builder.Services.AddHttpClient<IWalletService, WalletService>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = config["WalletStation:WalletBaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                }
            });

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminEmailService, AdminEmailService>();
            builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
            builder.Services.AddScoped<IUserSupportChatService, UserSupportChatService>();
            builder.Services.AddScoped<IAdminSupportChatService, AdminSupportChatService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IAdminUserService, AdminUserService>();
            builder.Services.AddScoped<IAdminTransactionService, AdminTransactionService>();
            builder.Services.AddScoped<IAdminReportService, AdminReportService>();
            builder.Services.AddScoped<IAdminSettingsService, AdminSettingsService>();
            builder.Services.AddSignalR();


            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapHub<SupportChatHub>("/supportChatHub");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Onboarding}/{id?}")
                .WithStaticAssets();

            if (app.Environment.IsDevelopment())
            {
                LegendPay.Data.AdminSeeder.SeedAsync(app.Services, app.Configuration).GetAwaiter().GetResult();
            }

            app.Run();
        }
    }
}