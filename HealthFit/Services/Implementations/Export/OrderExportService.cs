using ClosedXML.Excel;
using HealthFit.Models.DTO.Order;
using HealthFit.Services.Interfaces.Export;

namespace HealthFit.Services.Implementations.Export
{
    public class OrderExportService : IOrderExportService
    {
        public MemoryStream ExportOrdersToExcel(IEnumerable<OrderSearchResultDto> orders)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");

            worksheet.Cell(1, 1).Value = "Mã đơn hàng";
            worksheet.Cell(1, 2).Value = "Tên khách hàng";
            worksheet.Cell(1, 3).Value = "Ngày đặt";
            worksheet.Cell(1, 4).Value = "Tổng tiền";
            worksheet.Cell(1, 5).Value = "Trạng thái";

            int row = 2;
            foreach (var o in orders)
            {
                worksheet.Cell(row, 1).Value = o.OrderId;
                worksheet.Cell(row, 2).Value = o.UserName;
                worksheet.Cell(row, 3).Value = o.OrderDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 4).Value = o.TotalAmount;
                worksheet.Cell(row, 5).Value = o.Status;
                row++;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
