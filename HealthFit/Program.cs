using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Services;
using HealthFit.Services.Configurations;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserRoles.Services;

namespace HealthFit
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddDbContext<HealthyShopContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                // Cấu hình password requirements
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
                
                // Cấu hình user requirements
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<HealthyShopContext>()
                .AddDefaultTokenProviders();


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddApplicationServices();

            builder.Services.AddSingleton<OTPService>();
            builder.Services.AddScoped<SendMail>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
                    options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
                });
            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Store/Account/Login";
                options.AccessDeniedPath = "/Store/Account/Login";
                options.LogoutPath = "/Store/Account/Logout";
            });
            var app = builder.Build();
            app.UseSession();

            // Seed tài khoản SystemAdmin và các dữ liệu mặc định
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                await SeedService.SeedDatabase(serviceProvider);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Route mặc định vào Store (khi vào root)
            app.MapControllerRoute(
                name: "store_root",
                pattern: "",
                defaults: new { area = "Store", controller = "Home", action = "Index" }
            );

            // Route cho các area (Admin, Seller, Customer, etc.)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Route mặc định cho các controller ngoài area
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            // Route riêng cho Blog (Nutri area, viết ngắn)
            app.MapControllerRoute(
                name: "blog_default",
                pattern: "Blog/{action=Index}/{id?}",
                defaults: new { area = "Nutri", controller = "Blog" });

            app.MapRazorPages();

            app.Run();
        }
    }

}