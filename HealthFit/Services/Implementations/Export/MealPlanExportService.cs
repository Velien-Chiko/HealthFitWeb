using ClosedXML.Excel;
using HealthFit.Models;
using HealthFit.Services.Interfaces.Export;

namespace HealthFit.Services.Implementations.Export
{
    public class MealPlanExportService : IMealPlanExportService
    {
        public MemoryStream ExportMealPlansToExcel(IEnumerable<MealPlanDetail> mealPlans)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("MealPlans");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Mô tả";
            worksheet.Cell(1, 3).Value = "Phạm vi BMI";
            worksheet.Cell(1, 4).Value = "Trạng thái";

            int row = 2;
            foreach (var m in mealPlans)
            {
                worksheet.Cell(row, 1).Value = m.MealPlanDetailId;
                worksheet.Cell(row, 2).Value = m.PlanDescription;
                worksheet.Cell(row, 3).Value = m.Bmirange;
                worksheet.Cell(row, 4).Value = m.IsApproved switch
                {
                    "Approved" => "Đã duyệt",
                    "Pending" => "Chờ duyệt",
                    "Rejected" => "Bị từ chối",
                    _ => "Không xác định"
                };
                row++;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
