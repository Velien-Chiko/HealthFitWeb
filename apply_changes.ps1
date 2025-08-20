# Tạo cấu trúc thư mục
Write-Host "Dang tao cau truc thu muc..."
New-Item -ItemType Directory -Force -Path "Services\Interfaces"
New-Item -ItemType Directory -Force -Path "Services\Implementations"

# Tạo file IProductService.cs
Write-Host "Dang tao file IProductService.cs..."
$iProductServiceContent = @"
using HealthFit.Models;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace HealthFit.Services.Interfaces
{
    public interface IProductService
    {
        IPagedList<Product> SearchProducts(string searchString, bool? showHidden, string caloRange, int page);
        Product GetProductById(int id);
        void UpdateProduct(Product product, IFormFile? file);
        void ToggleProductStatus(int id);
        void DeleteProduct(int id);
    }
}
"@
Set-Content -Path "Services\Interfaces\IProductService.cs" -Value $iProductServiceContent

# Tạo file ProductService.cs
Write-Host "Dang tao file ProductService.cs..."
$productServiceContent = @"
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace HealthFit.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IPagedList<Product> SearchProducts(string searchString, bool? showHidden, string caloRange, int page)
        {
            var products = _unitOfWork.Product.Search(searchString, showHidden, caloRange);
            return products.ToPagedList(page, 5);
        }

        public Product GetProductById(int id)
        {
            return _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == id);
        }

        public void UpdateProduct(Product product, IFormFile? file)
        {
            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == product.ProductId);
            if (productFromDb == null)
            {
                throw new Exception("Không tìm thấy sản phẩm để cập nhật.");
            }

            if (file != null && file.Length > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                if (!string.IsNullOrEmpty(productFromDb.ImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                        productFromDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); }
                        catch { /* bỏ qua nếu lỗi */ }
                    }
                }

                productFromDb.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                product.ImageUrl = productFromDb.ImageUrl;
            }

            productFromDb.Name = product.Name;
            productFromDb.Description = product.Description;
            productFromDb.Ingredients = product.Ingredients;
            productFromDb.Price = product.Price;
            productFromDb.Quantity = product.Quantity;
            productFromDb.Calo = product.Calo;
            productFromDb.IsActive = product.IsActive;

            _unitOfWork.Product.Update(productFromDb);
            _unitOfWork.Save();
        }

        public void ToggleProductStatus(int id)
        {
            _unitOfWork.Product.ToggleStatus(id);
            _unitOfWork.Save();
        }

        public void DeleteProduct(int id)
        {
            _unitOfWork.Product.Delete(id);
            _unitOfWork.Save();
        }
    }
}
"@
Set-Content -Path "Services\Implementations\ProductService.cs" -Value $productServiceContent

# Cập nhật Program.cs
Write-Host "Dang cap nhat Program.cs..."
$programContent = @"
using HealthFit.Services.Interfaces;
using HealthFit.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
"@
Set-Content -Path "Program.cs" -Value $programContent

Write-Host "Hoan thanh!"
Read-Host "Nhan Enter de thoat..." 