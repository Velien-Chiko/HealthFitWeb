using X.PagedList;

namespace HealthFit.Models.ViewModels
{
    public class MealPlanSearchViewModel
    {
        public string SearchString { get; set; } = string.Empty;
        public string? BMI { get; set; }
        public string? ApprovalStatus { get; set; }
        public IPagedList<MealPlanDetail> MealPlans { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? PriceRange { get; set; }
    }
}