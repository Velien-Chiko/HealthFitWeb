using HealthFit.Models.DTO.Order;

namespace HealthFit.Services.Interfaces.Export
{
    public interface IOrderExportService
    {
        MemoryStream ExportOrdersToExcel(IEnumerable<OrderSearchResultDto> orders);
    }
}

