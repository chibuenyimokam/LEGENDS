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
            builder.Services.AddHttpClient<IBillerOneService, BillerOneService>();

            builder.Services.AddHttpClient<IWalletService, WalletService>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = config["WalletStation:WalletBaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                }
            });
            //builder.Services.AddHttpClient("WalletStationClient", client =>
            //{
            //    client.BaseAddress = new Uri("<WalletStation base URL>");
            //});

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IScheduledPaymentService, ScheduledPaymentService>();
            builder.Services.AddScoped<ILegendPointService, LegendPointService>();
            //builder.Services.AddScoped<IWalletTransactionHistoryService, WalletTransactionHistoryService>();
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

            //using scheme now cause we have admin and users on the same server and we want to avoid cookie breaking due to too many redirects cause it identifies admin and user as the same cookie and it will break the login flow for both parties


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "UserScheme";
            })
            .AddCookie("UserScheme", options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/Login";
                options.Cookie.Name = ".LegendPay.UserAuth";
                options.ExpireTimeSpan = TimeSpan.FromDays(1); //time user stays logged in
                options.SlidingExpiration = true; //instructs the server to re-issue a
                //new authentication cookie with a fresh expiration date whenever
                //a user makes a request while past the halfway point of the set ExpireTimeSpan
            })
            .AddCookie("AdminScheme", options =>
            {
                options.LoginPath = "/Admin/Login";
                options.LogoutPath = "/Admin/Logout";
                options.AccessDeniedPath = "/Admin/Login";
                options.Cookie.Name = ".LegendPay.AdminAuth";
                options.ExpireTimeSpan = TimeSpan.FromHours(2); //time user stays logged in
                options.SlidingExpiration = true; //instructs the server to re-issue a
                //new authentication cookie with a fresh expiration date whenever
                //a user makes a request while past the halfway point of the set ExpireTimeSpan
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapHub<SupportChatHub>("/supportChatHub");
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

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