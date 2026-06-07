using LegendPay.Interfaces;
using LegendPay.Models;
using LegendPay.Models.Data;    
using LegendPay.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using LegendPay.Interfaces.Admin;
using LegendPay.Services.Admin;

namespace LegendPay
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("default")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminEmailService, AdminEmailService>();
            builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

            //cookie authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Login";
                    options.LogoutPath = "/Admin/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                /*pattern: "{controller=Home}/{action=Index}/{id?}")*/
                pattern: "{controller=Home}/{action=Onboarding}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
