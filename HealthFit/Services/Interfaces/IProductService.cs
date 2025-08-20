using HealthFit.Models;
using HealthFit.Models.ViewModels;

namespace HealthFit.Services.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm trong hệ thống.
        /// </summary>
        Task<List<Product>> GetAllProductsAsync();

        /// <summary>
        /// Lấy thông tin chi tiết của một sản phẩm theo ID.
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        Task<Product> GetProductByIdAsync(int id);

        /// <summary>
        /// Tạo mới một sản phẩm, bao gồm cả upload ảnh nếu có.
        /// </summary>
        /// <param name="product">Thông tin sản phẩm</param>
        /// <param name="imageFile">Ảnh đại diện sản phẩm (tùy chọn)</param>
        Task<Product> CreateProductAsync(Product product, IFormFile? imageFile);


        /// <summary>
        /// Xóa một sản phẩm theo ID, đồng thời xóa ảnh khỏi thư mục nếu có.
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        Task DeleteProductAsync(int id);


        /// <summary>
        /// Tìm kiếm sản phẩm theo từ khóa, trạng thái, calo, giá và danh mục.
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên, mô tả, thành phần...)</param>
        /// <param name="showHidden">"Active" / "Inactive" / null</param>
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? showHidden, string? caloRange, string? priceRange, int? categoryId);


        /// <summary>
        /// Lấy thông tin chi tiết của một sản phẩm theo ID, bao gồm:
        /// tên sản phẩm, số lượng tồn kho, số lượng đã bán, calo và người tạo.
        /// </summary>
        /// <param name="id">ID của sản phẩm cần lấy thông tin chi tiết.</param>
        /// <returns>
        /// Một đối tượng <see cref="ProductDetailDto"/> chứa thông tin chi tiết của sản phẩm.
        /// Trả về null nếu không tìm thấy sản phẩm.
        /// </returns>
        Task<Models.DTO.Product.ProductDetailDto> GetProductDetailAsync(int id);

        Task<List<ProductCategory>> GetProductCategories();

        /// <summary>
        /// Cập nhật trạng thái mới cho sản phẩm theo ID .
        /// </summary>
        Task UpdateStatusAsync(int productId, string newStatus);

        /// <summary>
        /// Lấy ProductEditViewModel cho form chỉnh sửa
        /// </summary>
        Task<ProductEditViewModel> GetProductEditViewModelAsync(int productId);

        /// <summary>
        /// Cập nhật sản phẩm cơ bản: giá, số lượng, ảnh
        /// </summary>
        Task UpdateProductBasicInfoAsync(ProductEditViewModel model);

    }
}