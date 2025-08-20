using HealthFit.Models;

namespace HealthFit.Services.Interfaces.Export
{
    public interface IProductExportService
    {
        MemoryStream ExportProductsToExcel(IEnumerable<Product> products);
    }
}

