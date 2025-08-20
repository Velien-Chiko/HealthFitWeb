using HealthFit.Models;

namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface ICustomerProfileRepository : IRepository<CustomerProfile>
    {
        /// <summary>
        /// Lấy thông tin hồ sơ khách hàng theo UserId.
        /// </summary>
        Task<CustomerProfile?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Lấy hồ sơ khách hàng kèm theo thông tin người dùng (User) liên kết.
        /// </summary>
        Task<CustomerProfile?> GetByUserIdWithUserAsync(int userId);

        /// <summary>
        /// Kiểm tra hồ sơ có tồn tại theo ProfileId hay không.
        /// </summary>
        Task<bool> ExistsAsync(int profileId);

        /// <summary>
        /// Tạo mới hồ sơ khách hàng.
        /// </summary>
        Task<CustomerProfile> CreateAsync(CustomerProfile profile);

        /// <summary>
        /// Cập nhật hồ sơ khách hàng.
        /// </summary>
        Task<CustomerProfile> UpdateAsync(CustomerProfile profile);
    }
}