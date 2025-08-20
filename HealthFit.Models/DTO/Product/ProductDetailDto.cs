namespace HealthFit.Models.DTO.Product
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public int QuantitySold { get; set; }
        public int Calo { get; set; }
        public string? CreatedByName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }

    }
}
