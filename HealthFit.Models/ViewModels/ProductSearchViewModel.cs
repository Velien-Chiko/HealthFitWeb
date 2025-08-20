using X.PagedList;

namespace HealthFit.Models.ViewModels
{
    public class ProductSearchViewModel
    {
        public String SearchString { get; set; }
        public IPagedList<Product> ProductList { get; set; }
        public string? ShowHidden { get; set; }

        public string CaloRange { get; set; }

        public string PriceRange { get; set; }
        public int? CategoryId { get; set; }
        public List<ProductCategory> Categories { get; set; } = new();

        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
