using System.Text.Json.Serialization;

namespace HealthFit.Models.Models
{
    public class ProductCategoryMapping
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
        public ProductCategory Category { get; set; }

    }
}