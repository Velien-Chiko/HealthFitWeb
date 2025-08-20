using ClosedXML.Excel;
using HealthFit.Models;
using HealthFit.Services.Interfaces.Export;

namespace HealthFit.Services.Implementations.Export
{
    public class ProductExportService : IProductExportService
    {
        public MemoryStream ExportProductsToExcel(IEnumerable<Product> products)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Tên sản phẩm";
            worksheet.Cell(1, 3).Value = "Mô tả";
            worksheet.Cell(1, 4).Value = "Giá";
            worksheet.Cell(1, 5).Value = "Calo";
            worksheet.Cell(1, 6).Value = "Trạng thái";

            int row = 2;
            foreach (var p in products)
            {
                worksheet.Cell(row, 1).Value = p.ProductId;
                worksheet.Cell(row, 2).Value = p.Name;
                worksheet.Cell(row, 3).Value = p.Description;
                worksheet.Cell(row, 4).Value = p.Price;
                worksheet.Cell(row, 5).Value = p.Calo;
                worksheet.Cell(row, 6).Value = p.IsActive == "Active" ? "Kích hoạt" : "Ẩn";
                row++;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
