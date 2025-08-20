using Microsoft.AspNetCore.Identity; // Quản lý user, role của ASP.NET Core Identity
using HealthFit.DataAccess.Data; // DbContext của dự án
using HealthFit.Models; // Các model (User, ...)
using Microsoft.EntityFrameworkCore; // Entity Framework Core

// SeedService dùng để khởi tạo dữ liệu mặc định cho hệ thống khi ứng dụng khởi động lần đầu hoặc khi cần reset dữ liệu gốc.
namespace UserRoles.Services
{
    public class SeedService
    {
        // Phương thức SeedDatabase sẽ được gọi khi ứng dụng khởi động để khởi tạo dữ liệu mặc định
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            // Tạo scope dịch vụ để lấy các service cần thiết (DbContext, UserManager, RoleManager, Logger)
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HealthyShopContext>(); // Lấy DbContext
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>(); // Quản lý role
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>(); // Quản lý user
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>(); // Ghi log

            try
            {
                // Đảm bảo cơ sở dữ liệu đã được tạo và áp dụng các migrations
                logger.LogInformation("Đảm bảo cơ sở dữ liệu đã được tạo.");
                await context.Database.MigrateAsync();

                // Thêm các vai trò mặc định vào hệ thống
                logger.LogInformation("Bắt đầu tạo các vai trò (roles).");
                await AddRoleAsync(roleManager, "SystemAdmin"); // Tạo role SystemAdmin nếu chưa có
                await AddRoleAsync(roleManager, "Customer");   // Tạo role Customer nếu chưa có
                await AddRoleAsync(roleManager, "Admin");      // Tạo role Admin nếu chưa có
                await AddRoleAsync(roleManager, "Nutri");      // Tạo role Nutri nếu chưa có
                await AddRoleAsync(roleManager, "Seller");     // Tạo role Seller nếu chưa có
                await AddRoleAsync(roleManager, "Manager");    // Tạo role Manager nếu chưa có

                // Tạo tài khoản admin mặc định nếu chưa tồn tại
                logger.LogInformation("Bắt đầu tạo tài khoản admin.");
                var adminEmail = "admin@xbet.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)   // kiểm tra xem tài khoản admin đã tồn tại chưa
                {
                    // Tạo đối tượng user mới với thông tin cơ bản
                    var adminUser = new User
                    {
                        FullName = "System Admin",
                        UserName = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        EmailConfirmed = true, // Đánh dấu đã xác thực email
                        SecurityStamp = Guid.NewGuid().ToString(), // Mã bảo mật duy nhất
                    };

                    // Tạo user trong hệ thống với mật khẩu mặc định
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Gán vai trò SystemAdmin cho tài khoản admin.");
                        await userManager.AddToRoleAsync(adminUser, "SystemAdmin"); // Gán role SystemAdmin cho user
                    }
                    else
                    {
                        // Nếu tạo user thất bại thì ghi log lỗi chi tiết
                        logger.LogError("Không thể tạo tài khoản admin: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log nếu có lỗi xảy ra trong quá trình seed dữ liệu
                logger.LogError(ex, "Đã xảy ra lỗi trong quá trình seed database.");
            }
        }

        // Phương thức hỗ trợ tạo role nếu role chưa tồn tại
        private static async Task AddRoleAsync(RoleManager<IdentityRole<int>> roleManager, string roleName)
        {
            // Kiểm tra nếu role chưa tồn tại thì tạo mới
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!result.Succeeded)
                {
                    // Nếu thất bại, ném lỗi kèm theo thông tin chi tiết
                    throw new Exception($"Không thể tạo role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        //Là để đảm bảo rằng vai trò (role) như "Admin", "Seller", "Nutri" đã tồn tại trong hệ thống. Nếu chưa có thì tự động tạo mới vai trò đó trong cơ sở dữ liệu.
    }
}
