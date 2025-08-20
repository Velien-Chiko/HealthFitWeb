using System.Text.Json.Serialization;
using HealthFit.Models.Models;

namespace HealthFit.Models;

public partial class ProductCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }
    [JsonIgnore]
    public virtual ICollection<ProductCategoryMapping> ProductCategoryMappings { get; set; } = new List<ProductCategoryMapping>();

}
