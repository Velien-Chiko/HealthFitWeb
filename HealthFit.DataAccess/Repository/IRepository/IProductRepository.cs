using System.Linq.Expressions;
using HealthFit.Models;

namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Models.Product>
    {
        void Update(Product obj);
        /// <summary>
        /// Lấy bản ghi sản phẩm đầu tiên thỏa mãn điều kiện lọc.
        /// </summary>
        /// <param name="filter">Biểu thức điều kiện lọc</param>
        /// <returns>Sản phẩm đầu tiên thỏa mãn hoặc null nếu không có</returns>
        Task<Product?> GetFirstOrDefaultAsync(Expression<Func<Product, bool>> filter);

        /// <summary>
        /// Tìm sản phẩm theo ID.
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <returns>Sản phẩm tìm được hoặc null nếu không có</returns>
        Task<Product?> FindByIdAsync(int id);

        /// <summary>
        /// Lấy toàn bộ danh sách sản phẩm.
        /// </summary>
        /// <returns>Danh sách tất cả sản phẩm</returns>
        Task<List<Product>> GetAllAsync();


        /// <summary>
        /// Lọc sản phẩm theo trạng thái hiển thị ("Active", "Inactive", null = tất cả)
        /// </summary>
        /// <param name="showHidden">Trạng thái cần lọc</param>
        Task<IEnumerable<Product>> GetProductsFilteredAsync(string? showHidden);

        /// <summary>
        /// Tìm kiếm sản phẩm theo từ khóa, trạng thái, calo, giá và danh mục.
        /// </summary>
        /// <param name="search">Từ khóa</param>
        /// <param name="showHidden">Trạng thái hiển thị sản phẩm</param>
        /// <param name="caloRange">Khoảng calo (VD: "201-300")</param>
        /// <param name="priceRange">Khoảng giá (VD: "0-20000")</param>
        /// <param name="categoryId">ID danh mục</param>
        Task<IEnumerable<Product>> SearchAsync(string search, string? showHidden, string? caloRange, string? priceRange, int? categoryId);

        /// <summary>
        /// Cập nhật thông tin một sản phẩm.
        /// </summary>
        /// <param name="product">Đối tượng sản phẩm cần cập nhật</param>
        Task UpdateAsync(Product product);

        /// <summary>
        /// Xóa sản phẩm theo ID.
        /// </summary>
        /// <param name="id">ID của sản phẩm cần xóa</param>
        Task DeleteAsync(int id);


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
    }
}
