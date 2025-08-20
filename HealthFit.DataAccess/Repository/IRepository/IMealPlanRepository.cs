using HealthFit.Models;
using HealthFit.Models.DTO.MealPlan;

namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface IMealPlanRepository : IRepository<MealPlanDetail>
    {
        /// <summary>
        /// Lấy thông tin chi tiết của một kế hoạch bữa ăn, bao gồm danh sách các sản phẩm thuộc kế hoạch đó,
        /// dựa trên ID của kế hoạch bữa ăn.
        /// </summary>
        /// <param name="mealPlanId">ID của kế hoạch bữa ăn cần lấy.</param>
        /// <returns>
        /// Một đối tượng <see cref="MealPlanDto"/> chứa thông tin kế hoạch bữa ăn và danh sách sản phẩm tương ứng.
        /// </returns>
        Task<MealPlanDto> GetMealPlanWithProductsAsync(int mealPlanId);

        /// <summary>
        /// Lấy toàn bộ danh sách chi tiết kế hoạch bữa ăn từ cơ sở dữ liệu.
        /// </summary>
        /// <returns>
        /// Danh sách tất cả các đối tượng <see cref="MealPlanDetail"/> hiện có.
        /// </returns>
        Task<IEnumerable<MealPlanDetail>> GetAllMealPlanDetailsAsync();

        /// <summary>
        /// Lấy danh sách kế hoạch bữa ăn đã được lọc theo trạng thái phê duyệt.
        /// </summary>
        /// <param name="isApproved">
        /// Trạng thái cần lọc:
        /// - "Approved", "Pending", "Rejected"
        /// - null: lấy tất cả không phân biệt trạng thái
        /// </param>
        /// <returns>
        /// Danh sách các <see cref="MealPlanDetail"/> phù hợp với điều kiện lọc.
        /// </returns>
        Task<IEnumerable<MealPlanDetail>> GetMealPlanFilteredAsync(string? isApproved);

        /// <summary>
        /// Tìm kiếm danh sách chi tiết kế hoạch bữa ăn theo từ khóa, chỉ số BMI và trạng thái phê duyệt.
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm (ví dụ theo tên món ăn hoặc mô tả).</param>
        /// <param name="BMI">Chỉ số BMI cần lọc (tùy chọn).</param>
        /// <param name="isApproved">Trạng thái phê duyệt (Approved / Pending / Rejected / null để bỏ lọc).</param>
        /// <param name="priceRange">Khoảng giá (nếu có).</param>
        /// <returns>
        /// Danh sách các <see cref="MealPlanDetail"/> thỏa mãn điều kiện tìm kiếm.
        /// </returns>
        Task<IEnumerable<MealPlanDetail>> SearchMealPlanDetailsAsync(string keyword, string? BMI, string? isApproved, string? priceRange);

        /// <summary>
        /// Cập nhật trạng thái mới cho kế hoạch bữa ăn theo ID.
        /// </summary>
        /// <param name="id">ID của kế hoạch bữa ăn cần thay đổi.</param>
        /// <param name="newStatus">Trạng thái mới (Pending, Approved, Rejected).</param>
        Task UpdateStatusMealPlanAsync(int id, string newStatus);


        /// <summary>
        /// Lấy trạng thái phê duyệt của một kế hoạch bữa ăn theo ID.
        /// </summary>
        /// <param name="id">ID của kế hoạch bữa ăn cần kiểm tra.</param>
        /// <returns>
        /// Chuỗi trạng thái phê duyệt: "Approved", "Pending", "Rejected" hoặc null nếu không tìm thấy.
        /// </returns>
        Task<string?> GetIsApprovedAsync(int id);

        /// <summary>
        /// Tính số tiền tiết kiệm được khi mua một Meal Plan so với việc mua lẻ từng sản phẩm trong combo.
        /// </summary>
        /// <param name="mealPlanId">ID của Meal Plan cần tính toán.</param>
        /// <returns>
        /// Số tiền tiết kiệm được (tổng giá lẻ của các sản phẩm - giá bán của Meal Plan).  
        /// Nếu không tiết kiệm hoặc dữ liệu không hợp lệ, trả về 0.
        /// </returns>
        Task<decimal> CalculateSavingsForMealPlanAsync(int mealPlanId);

        /// <summary>
        /// Cập nhật thông tin kế hoạch bữa ăn bao gồm giá, trạng thái và hình ảnh.
        /// </summary>
        /// <param name="mealPlanId">ID của kế hoạch bữa ăn cần cập nhật</param>
        /// <param name="newPrice">Giá mới của kế hoạch (đơn vị đồng); null nếu không cập nhật</param>
        /// <param name="newStatus">Trạng thái mới (Approved, Pending, Rejected); null nếu không cập nhật</param>
        /// <param name="newImageUrl">Đường dẫn ảnh mới; null nếu không cập nhật</param>
        /// <returns>Task bất đồng bộ</returns>
        Task UpdateMealPlanInfoAsync(int mealPlanId, decimal? newPrice, string? newStatus, string? newImageUrl);


        /// <summary>
        /// Tìm kế hoạch bữa ăn theo ID.
        /// </summary>
        /// <param name="id">ID của MealPlan</param>
        /// <returns>MealPlanDetail nếu tìm thấy, ngược lại null</returns>
        Task<MealPlanDetail?> FindByIdAsync(int id);

        /// <summary>
        /// Cập nhật thông tin một đối tượng MealPlanDetail.
        /// </summary>
        /// <param name="mealPlan">Đối tượng cần cập nhật</param>
        Task UpdateAsync(MealPlanDetail mealPlan);

        void Update(CartItem obj);

    }
}
