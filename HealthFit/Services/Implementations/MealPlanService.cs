using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.Constants;
using HealthFit.Models.DTO.MealPlan;
using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;

namespace HealthFit.Services.Implementations
{
    public class MealPlanService : IMealPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MealPlanService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public MealPlanService(IUnitOfWork unitOfWork, ILogger<MealPlanService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<decimal> CalculateSavingsForMealPlanAsync(int mealPlanId)
        {
            if (mealPlanId <= 0)
            {
                throw new ArgumentException("ID của Meal Plan không hợp lệ.");
            }
            var savings = await _unitOfWork.MealPlan.CalculateSavingsForMealPlanAsync(mealPlanId);
            if (savings == 0)
            {
                _logger.LogInformation($"MealPlan {mealPlanId} không tiết kiệm được đồng nào hoặc không tồn tại.");
            }
            return savings;
        }

        public async Task<IEnumerable<MealPlanDetail>> GetAllMealPlanDetailsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all meal plan details");
                return await _unitOfWork.MealPlan.GetAllMealPlanDetailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all meal plan details");
                throw;
            }
        }

        public async Task<string?> GetIsApprovedAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting approval status for meal plan ID: {MealPlanId}", id);
                return await _unitOfWork.MealPlan.GetIsApprovedAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval status for meal plan ID: {MealPlanId}", id);
                throw;
            }
        }

        public async Task<MealPlanEditViewModel> GetMealPlanEditByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation(" Đang lấy dữ liệu MealPlan để chỉnh sửa: ID = {Id}", id);

                var mealPlan = await _unitOfWork.MealPlan.FindByIdAsync(id);
                if (mealPlan == null)
                {
                    _logger.LogWarning(" Không tìm thấy MealPlan với ID: {Id}", id);
                    throw new KeyNotFoundException($"Không tìm thấy MealPlan với ID {id}");
                }

                return new MealPlanEditViewModel
                {
                    MealPlanId = mealPlan.MealPlanDetailId,
                    PlanDescription = mealPlan.PlanDescription,
                    Price = mealPlan.Price / 1000,
                    CurrentImageUrl = mealPlan.ImageUrl,
                    IsApproved = mealPlan.IsApproved
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Lỗi khi lấy MealPlan để edit: ID = {Id}", id);
                throw;
            }
        }


        public async Task<IEnumerable<MealPlanDetail>> GetMealPlanFilteredAsync(string? isApproved)
        {
            try
            {
                _logger.LogInformation("Getting filtered meal plans with isApproved: {IsApproved}", isApproved);
                return await _unitOfWork.MealPlan.GetMealPlanFilteredAsync(isApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered meal plans");
                throw;
            }
        }

        public async Task<MealPlanDto> GetMealPlanWithProductsAsync(int mealPlanId)
        {
            try
            {
                _logger.LogInformation("Getting meal plan with products for ID: {MealPlanId}", mealPlanId);
                var mealPlan = await _unitOfWork.MealPlan.GetMealPlanWithProductsAsync(mealPlanId);
                if (mealPlan == null)
                {
                    _logger.LogWarning("Meal plan not found for ID: {MealPlanId}", mealPlanId);
                    throw new KeyNotFoundException($"Meal plan with ID {mealPlanId} not found");
                }
                return mealPlan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meal plan with products for ID: {MealPlanId}", mealPlanId);
                throw;
            }
        }

        public async Task<IEnumerable<MealPlanDetail>> SearchMealPlanDetailsAsync(string keyword, string? BMI, string? isApproved, string? priceRange)
        {
            try
            {
                _logger.LogInformation("Searching meal plans with keyword: {Keyword}, BMI: {BMI}, isApproved: {IsApproved}",
                    keyword, BMI, isApproved);

                float? parsedBmi = null;
                if (!string.IsNullOrEmpty(BMI))
                {
                    if (!float.TryParse(BMI, out float bmiValue))
                    {
                        throw new ArgumentException("Giá trị BMI không hợp lệ. Vui lòng nhập một số như 18.5 hoặc 23.0.");
                    }

                    if (bmiValue < 0)
                    {
                        throw new ArgumentException("BMI không được là số âm. Vui lòng nhập giá trị hợp lệ.");
                    }

                    parsedBmi = bmiValue;
                }

                return await _unitOfWork.MealPlan.SearchMealPlanDetailsAsync(keyword, BMI, isApproved, priceRange);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Dữ liệu đầu vào không hợp lệ khi tìm kiếm Meal Plan.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching meal plans");
                throw;
            }
        }

        public async Task UpdateMealPlanInfoAsync(MealPlanEditViewModel model)
        {
            _logger.LogInformation("Bắt đầu cập nhật MealPlan ID = {Id}", model.MealPlanId);

            var mealPlan = await _unitOfWork.MealPlan.FindByIdAsync(model.MealPlanId);
            if (mealPlan == null)
                throw new Exception("Không tìm thấy kế hoạch bữa ăn.");

            // Validate status
            if (!string.IsNullOrWhiteSpace(model.IsApproved) && !ProductStatusConstants.All.Contains(model.IsApproved))
            {
                _logger.LogWarning("Trạng thái không hợp lệ: {Status}", model.IsApproved);
                throw new ArgumentException("Trạng thái không hợp lệ.");
            }

            mealPlan.Price = model.Price * 1000;
            mealPlan.IsApproved = model.IsApproved;

            // Xử lý ảnh mới nếu có
            if (model.NewImage != null)
            {
                var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(model.NewImage.FileName).ToLower();

                if (!validExtensions.Contains(ext))
                {
                    _logger.LogWarning("Ảnh không hợp lệ: {Ext}", ext);
                    throw new ArgumentException("Ảnh phải là .jpg, .jpeg hoặc .png");
                }

                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(mealPlan.ImageUrl))
                    DeleteImage(mealPlan.ImageUrl, "mealplan");

                // Lưu ảnh mới
                mealPlan.ImageUrl = await SaveImageAsync(model.NewImage, "mealplan");
            }

            await _unitOfWork.MealPlan.UpdateAsync(mealPlan);
            _unitOfWork.Save();

            _logger.LogInformation("✅ Cập nhật MealPlan thành công");
        }




        public async Task UpdateStatusMealPlanAsync(int id, string newStatus)
        {
            try
            {
                _logger.LogInformation("Cập nhật trạng thái kế hoạch bữa ăn: ID = {MealPlanId}, Status = {Status}", id, newStatus);

                // ✅ Dùng chung constants
                if (!ProductStatusConstants.All.Contains(newStatus))
                {
                    _logger.LogWarning("Trạng thái không hợp lệ: {Status}", newStatus);
                    throw new ArgumentException("Trạng thái không hợp lệ.");
                }

                await _unitOfWork.MealPlan.UpdateStatusMealPlanAsync(id, newStatus);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái MealPlan ID: {MealPlanId}", id);
                throw;
            }
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string uploadPath = Path.Combine(wwwRootPath, "images", folder);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string fullPath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{folder}/{fileName}";
        }

        private void DeleteImage(string imageUrl, string folder)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string relativePath = imageUrl.TrimStart('/');
            string fullPath = Path.Combine(wwwRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }



    }
}
