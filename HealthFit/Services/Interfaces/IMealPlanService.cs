using HealthFit.Models;
using HealthFit.Models.DTO.MealPlan;
using HealthFit.Models.ViewModels;

namespace HealthFit.Services.Interfaces
{
    public interface IMealPlanService
    {
        /// <summary>
        /// Lấy toàn bộ chi tiết các kế hoạch bữa ăn.
        /// </summary>
        Task<IEnumerable<MealPlanDetail>> GetAllMealPlanDetailsAsync();

        /// <summary>
        /// Lọc danh sách kế hoạch bữa ăn theo trạng thái phê duyệt.
        /// </summary>
        /// <param name="isApproved">Giá trị: "Approved", "Pending", "Rejected" hoặc null nếu không lọc</param>
        Task<IEnumerable<MealPlanDetail>> GetMealPlanFilteredAsync(string? isApproved);

        /// <summary>
        /// Lấy thông tin kế hoạch bữa ăn cùng danh sách sản phẩm tương ứng theo ID.
        /// </summary>
        /// <param name="mealPlanId">ID của kế hoạch bữa ăn</param>
        Task<MealPlanDto> GetMealPlanWithProductsAsync(int mealPlanId);

        /// <summary>
        /// Tìm kiếm kế hoạch bữa ăn theo từ khóa, chỉ số BMI và trạng thái duyệt.
        /// </summary>
        /// <param name="keyword">Từ khóa trong phần mô tả</param>
        /// <param name="BMI">Chỉ số BMI để lọc theo phạm vi phù hợp</param>
        /// <param name="isApproved">Trạng thái duyệt: "Approved", "Pending", "Rejected" hoặc null</param>
        /// <param name="priceRange">Khoảng giá</param>
        Task<IEnumerable<MealPlanDetail>> SearchMealPlanDetailsAsync(string keyword, string? BMI, string? isApproved, string? priceRange);

        /// <summary>
        /// Cập nhật trạng thái phê duyệt mới cho Meal Plan
        /// </summary>
        /// <param name="id">ID của kế hoạch bữa ăn</param>
        /// <param name="newStatus">Trạng thái mới: Approved, Pending, Rejected</param>
        Task UpdateStatusMealPlanAsync(int id, string newStatus);


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
        /// Cập nhật thông tin kế hoạch bữa ăn sau khi kiểm tra hợp lệ đầu vào.
        /// Cho phép cập nhật giá, trạng thái và ảnh. Nếu trường nào null thì giữ nguyên.
        /// </summary>
        /// <param name="mealPlanId">ID của kế hoạch bữa ăn</param>
        /// <param name="newPrice">Giá mới (đơn vị đồng)</param>
        /// <param name="newStatus">Trạng thái mới: "Approved", "Pending", "Rejected"</param>
        /// <param name="newImageUrl">URL hình ảnh mới</param>
        Task UpdateMealPlanInfoAsync(MealPlanEditViewModel model);
        /// <summary>
        /// Lấy thông tin chi tiết của kế hoạch bữa ăn để phục vụ chỉnh sửa (Edit).
        /// </summary>
        /// <param name="id">ID của MealPlan</param>
        /// <returns>MealPlanEditViewModel nếu tìm thấy, ngược lại ném lỗi</returns>
        Task<MealPlanEditViewModel> GetMealPlanEditByIdAsync(int id);


    }
}
