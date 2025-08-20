using HealthFit.DataAccess.Interfaces;
using HealthFit.Models;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace HealthFit.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<Product>> SearchProducts(string searchString)
        {
            var products = _unitOfWork.Product.Search(searchString, null, null);
            return products.ToList();
        }

        public async Task<Product?> GetProductById(int id)
        {
            return _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == id);
        }

        public async Task<bool> UpdateProduct(Product product, IFormFile? imageFile)
        {
            try
            {
                if (imageFile != null)
                {
                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleProductStatus(int id)
        {
            try
            {
                _unitOfWork.Product.ToggleStatus(id);
                _unitOfWork.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProduct(int id)
        {
            try
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == id);
                if (product != null)
                {
                    // Xóa ảnh sản phẩm
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (File.Exists(imagePath))
                        {
                            File.Delete(imagePath);
                        }
                    }

                    _unitOfWork.Product.Delete(id);
                    _unitOfWork.Save();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
} 