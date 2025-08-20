using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.Constants;
using HealthFit.Models.DTO.Product;
using HealthFit.Models.ViewModels;
using HealthFit.Services.Interfaces;

namespace HealthFit.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductService> _logger;
        private readonly string _baseUrl;

        public ProductService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<ProductService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:7126";
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _unitOfWork.Product.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _unitOfWork.Product.FindByIdAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product, IFormFile? imageFile)
        {
            // Validate: Tên sản phẩm không được trùng (không phân biệt hoa thường)
            var existingProduct = (await _unitOfWork.Product.GetAllAsync())
                .FirstOrDefault(p => p.Name.ToLower() == product.Name.ToLower());
            if (existingProduct != null)
            {
                throw new Exception("Tên sản phẩm đã tồn tại.");
            }

            // Validate: Giá phải lớn hơn 0
            if (product.Price <= 0)
            {
                throw new Exception("Giá sản phẩm phải lớn hơn 0.");
            }

            // Validate: Số lượng phải >= 0
            if (product.Quantity < 0)
            {
                throw new Exception("Số lượng sản phẩm không được nhỏ hơn 0.");
            }

            // Validate: Calo phải >= 0
            if (product.Calo < 0)
            {
                throw new Exception("Chỉ số calo không được nhỏ hơn 0.");
            }

            // Validate: File ảnh (nếu có) phải là ảnh
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                throw new Exception("File tải lên phải là ảnh.");
            }

            // Nếu qua hết validate thì tiếp tục logic cũ
            if (imageFile != null)
            {
                product.ImageUrl = await SaveImageAsync(imageFile);
            }

            _unitOfWork.Product.Add(product);
            _unitOfWork.Save();

            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa sản phẩm với ID: {ProductId}", id);

            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    _logger.LogInformation("Xóa ảnh sản phẩm: {ImageUrl}", product.ImageUrl);
                    DeleteImage(product.ImageUrl);
                }

                await _unitOfWork.Product.DeleteAsync(id);
                _unitOfWork.Save();

                _logger.LogInformation("Đã xóa sản phẩm với ID: {ProductId}", id);
            }
            else
            {
                _logger.LogWarning("Không tìm thấy sản phẩm để xóa với ID: {ProductId}", id);
            }
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? showHidden, string? caloRange, string? priceRange, int? categoryId)
        {
            _logger.LogInformation("Tìm kiếm sản phẩm với từ khóa: {SearchTerm}, Trạng thái: {ShowHidden}, Calo: {CaloRange}", searchTerm, showHidden, caloRange);
            return await _unitOfWork.Product.SearchAsync(searchTerm, showHidden, caloRange, priceRange, categoryId);
        }


        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string productPath = Path.Combine(wwwRootPath, "images", "product");

            if (!Directory.Exists(productPath))
            {
                Directory.CreateDirectory(productPath);
                _logger.LogInformation("Tạo thư mục lưu ảnh sản phẩm: {ProductPath}", productPath);
            }

            string fullPath = Path.Combine(productPath, fileName);
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            string imageUrl = $"/images/product/{fileName}";
            _logger.LogInformation("Đã lưu ảnh sản phẩm: {ImageUrl}", imageUrl);

            return imageUrl;
        }



        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogWarning("ImageUrl rỗng, không thể xóa ảnh");
                return;
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string relativePath = imageUrl.TrimStart('/');

            var fullPath = Path.Combine(wwwRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                _logger.LogInformation("Đã xóa file ảnh: {FullPath}", fullPath);
            }
            else
            {
                _logger.LogWarning("Không tìm thấy file ảnh để xóa: {FullPath}", fullPath);
            }
        }

        public async Task<ProductDetailDto> GetProductDetailAsync(int id)
        {
            _logger.LogInformation("Lấy chi tiết sản phẩm : {ShowHidden}", id);
            return await _unitOfWork.Product.GetProductDetailAsync(id);

        }

        public async Task<List<ProductCategory>> GetProductCategories()
        {
            return await _unitOfWork.Product.GetProductCategories();
        }

        public async Task UpdateStatusAsync(int productId, string newStatus)
        {
            if (!ProductStatusConstants.All.Contains(newStatus))
                throw new ArgumentException("Invalid status");

            await _unitOfWork.Product.UpdateStatusAsync(productId, newStatus);
            _unitOfWork.Save();
        }

        public async Task<ProductEditViewModel> GetProductEditViewModelAsync(int productId)
        {
            var product = await _unitOfWork.Product.FindByIdAsync(productId);
            if (product == null)
                throw new Exception("Không tìm thấy sản phẩm");

            return new ProductEditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price / 1000,
                Quantity = product.Quantity,
                CurrentImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };
        }

        public async Task UpdateProductBasicInfoAsync(ProductEditViewModel model)
        {
            var product = await _unitOfWork.Product.FindByIdAsync(model.ProductId);
            if (product == null)
                throw new Exception("Không tìm thấy sản phẩm");

            product.Price = model.Price * 1000;
            product.Quantity = model.Quantity;

            if (!string.IsNullOrEmpty(model.IsActive))
            {
                product.IsActive = model.IsActive;
            }
            if (model.NewImage != null)
            {
                if (!model.NewImage.ContentType.StartsWith("image/"))
                    throw new Exception("File phải là ảnh");

                var newImageUrl = await SaveImageAsync(model.NewImage);

                if (!string.IsNullOrEmpty(product.ImageUrl))
                    DeleteImage(product.ImageUrl);

                product.ImageUrl = newImageUrl;
            }





            await _unitOfWork.Product.UpdateAsync(product);
            _unitOfWork.Save();
        }


    }

}

