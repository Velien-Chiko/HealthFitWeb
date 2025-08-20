using HealthFit.Models;

namespace HealthFit.Services.Interfaces
{
    public interface ICustomerProfileService
    {
        /// <summary>
        /// Lấy hồ sơ khách hàng theo UserId (không kèm thông tin user).
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Hồ sơ khách hàng hoặc null nếu không tồn tại.</returns>
        Task<CustomerProfile?> GetProfileByUserIdAsync(int userId);

        /// <summary>
        /// Lấy hồ sơ khách hàng kèm theo thông tin người dùng liên kết.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Hồ sơ khách hàng có bao gồm User hoặc null nếu không tồn tại.</returns>
        Task<CustomerProfile?> GetProfileByUserIdWithUserAsync(int userId);

        /// <summary>
        /// Tạo mới hồ sơ khách hàng.
        /// </summary>
        /// <param name="profile">Thông tin hồ sơ cần tạo.</param>
        /// <returns>Hồ sơ khách hàng sau khi tạo thành công.</returns>
        Task<CustomerProfile> CreateProfileAsync(CustomerProfile profile);

        /// <summary>
        /// Cập nhật hồ sơ khách hàng (chỉ cập nhật thông tin được phép chỉnh sửa).
        /// </summary>
        /// <param name="profile">Thông tin cập nhật.</param>
        /// <param name="userId">ID người dùng yêu cầu cập nhật.</param>
        /// <returns>Hồ sơ khách hàng sau khi cập nhật thành công.</returns>
        Task<CustomerProfile> UpdateProfileAsync(CustomerProfile profile, int userId);

        /// <summary>
        /// Kiểm tra hồ sơ có tồn tại theo ProfileId hay không.
        /// </summary>
        /// <param name="profileId">ID của hồ sơ.</param>
        /// <returns>True nếu tồn tại, ngược lại false.</returns>
        Task<bool> ProfileExistsAsync(int profileId);

        /// <summary>
        /// Lấy hồ sơ khách hàng theo userId, nếu chưa có thì tự động tạo mới.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Hồ sơ khách hàng hiện tại hoặc hồ sơ mới được tạo.</returns>
        Task<CustomerProfile> GetOrCreateProfileAsync(int userId);
    }
}