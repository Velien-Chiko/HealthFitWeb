using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HealthFit.Models.ViewModels
{
    public class MealPlanEditViewModel
    {
        public int MealPlanId { get; set; }
        public string? PlanDescription { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }
        public string? CurrentImageUrl { get; set; }
        public IFormFile? NewImage { get; set; }
        public string? IsApproved { get; set; }
    }
}
