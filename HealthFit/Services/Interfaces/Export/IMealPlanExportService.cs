using HealthFit.Models;

namespace HealthFit.Services.Interfaces.Export
{
    public interface IMealPlanExportService
    {
        MemoryStream ExportMealPlansToExcel(IEnumerable<MealPlanDetail> mealPlans);
    }
}
