@echo off
echo Dang tao cau truc thu muc...
mkdir Services\Interfaces
mkdir Services\Implementations

echo Dang tao file IProductService.cs...
echo using HealthFit.Models; > Services\Interfaces\IProductService.cs
echo using Microsoft.AspNetCore.Http; >> Services\Interfaces\IProductService.cs
echo using X.PagedList; >> Services\Interfaces\IProductService.cs
echo. >> Services\Interfaces\IProductService.cs
echo namespace HealthFit.Services.Interfaces >> Services\Interfaces\IProductService.cs
echo { >> Services\Interfaces\IProductService.cs
echo     public interface IProductService >> Services\Interfaces\IProductService.cs
echo     { >> Services\Interfaces\IProductService.cs
echo         IPagedList^<Product^> SearchProducts(string searchString, bool? showHidden, string caloRange, int page); >> Services\Interfaces\IProductService.cs
echo         Product GetProductById(int id); >> Services\Interfaces\IProductService.cs
echo         void UpdateProduct(Product product, IFormFile? file); >> Services\Interfaces\IProductService.cs
echo         void ToggleProductStatus(int id); >> Services\Interfaces\IProductService.cs
echo         void DeleteProduct(int id); >> Services\Interfaces\IProductService.cs
echo     } >> Services\Interfaces\IProductService.cs
echo } >> Services\Interfaces\IProductService.cs

echo Dang tao file ProductService.cs...
echo using HealthFit.DataAccess.Repository.IRepository; > Services\Implementations\ProductService.cs
echo using HealthFit.Models; >> Services\Implementations\ProductService.cs
echo using HealthFit.Services.Interfaces; >> Services\Implementations\ProductService.cs
echo using Microsoft.AspNetCore.Http; >> Services\Implementations\ProductService.cs
echo using X.PagedList; >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo namespace HealthFit.Services.Implementations >> Services\Implementations\ProductService.cs
echo { >> Services\Implementations\ProductService.cs
echo     public class ProductService : IProductService >> Services\Implementations\ProductService.cs
echo     { >> Services\Implementations\ProductService.cs
echo         private readonly IUnitOfWork _unitOfWork; >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo         public ProductService(IUnitOfWork unitOfWork) >> Services\Implementations\ProductService.cs
echo         { >> Services\Implementations\ProductService.cs
echo             _unitOfWork = unitOfWork; >> Services\Implementations\ProductService.cs
echo         } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo         public IPagedList^<Product^> SearchProducts(string searchString, bool? showHidden, string caloRange, int page) >> Services\Implementations\ProductService.cs
echo         { >> Services\Implementations\ProductService.cs
echo             var products = _unitOfWork.Product.Search(searchString, showHidden, caloRange); >> Services\Implementations\ProductService.cs
echo             return products.ToPagedList(page, 5); >> Services\Implementations\ProductService.cs
echo         } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo         public Product GetProductById(int id) >> Services\Implementations\ProductService.cs
echo         { >> Services\Implementations\ProductService.cs
echo             return _unitOfWork.Product.GetFirstOrDefault(p =^> p.ProductId == id); >> Services\Implementations\ProductService.cs
echo         } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo         public void UpdateProduct(Product product, IFormFile? file) >> Services\Implementations\ProductService.cs
echo         { >> Services\Implementations\ProductService.cs
echo             var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p =^> p.ProductId == product.ProductId); >> Services\Implementations\ProductService.cs
echo             if (productFromDb == null) >> Services\Implementations\ProductService.cs
echo             { >> Services\Implementations\ProductService.cs
echo                 throw new Exception("Không tìm thấy sản phẩm để cập nhật."); >> Services\Implementations\ProductService.cs
echo             } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo             if (file != null ^&^& file.Length ^> 0) >> Services\Implementations\ProductService.cs
echo             { >> Services\Implementations\ProductService.cs
echo                 var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products"); >> Services\Implementations\ProductService.cs
echo                 if (!Directory.Exists(uploadFolder)) >> Services\Implementations\ProductService.cs
echo                     Directory.CreateDirectory(uploadFolder); >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo                 var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); >> Services\Implementations\ProductService.cs
echo                 var filePath = Path.Combine(uploadFolder, fileName); >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo                 using (var stream = new FileStream(filePath, FileMode.Create)) >> Services\Implementations\ProductService.cs
echo                 { >> Services\Implementations\ProductService.cs
echo                     file.CopyTo(stream); >> Services\Implementations\ProductService.cs
echo                 } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo                 if (!string.IsNullOrEmpty(productFromDb.ImageUrl)) >> Services\Implementations\ProductService.cs
echo                 { >> Services\Implementations\ProductService.cs
echo                     var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productFromDb.ImageUrl.TrimStart('/')); >> Services\Implementations\ProductService.cs
echo                     if (System.IO.File.Exists(oldFilePath)) >> Services\Implementations\ProductService.cs
echo                     { >> Services\Implementations\ProductService.cs
echo                         try { System.IO.File.Delete(oldFilePath); } >> Services\Implementations\ProductService.cs
echo                         catch { /* bỏ qua nếu lỗi */ } >> Services\Implementations\ProductService.cs
echo                     } >> Services\Implementations\ProductService.cs
echo                 } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo                 productFromDb.ImageUrl = "/images/products/" + fileName; >> Services\Implementations\ProductService.cs
echo             } >> Services\Implementations\ProductService.cs
echo             else >> Services\Implementations\ProductService.cs
echo             { >> Services\Implementations\ProductService.cs
echo                 product.ImageUrl = productFromDb.ImageUrl; >> Services\Implementations\ProductService.cs
echo             } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo             productFromDb.Name = product.Name; >> Services\Implementations\ProductService.cs
echo             productFromDb.Description = product.Description; >> Services\Implementations\ProductService.cs
echo             productFromDb.Ingredients = product.Ingredients; >> Services\Implementations\ProductService.cs
echo             productFromDb.Price = product.Price; >> Services\Implementations\ProductService.cs
echo             productFromDb.Quantity = product.Quantity; >> Services\Implementations\ProductService.cs
echo             productFromDb.Calo = product.Calo; >> Services\Implementations\ProductService.cs
echo             productFromDb.IsActive = product.IsActive; >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo             _unitOfWork.Product.Update(productFromDb); >> Services\Implementations\ProductService.cs
echo             _unitOfWork.Save(); >> Services\Implementations\ProductService.cs
echo         } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo         public void ToggleProductStatus(int id) >> Services\Implementations\ProductService.cs
echo         { >> Services\Implementations\ProductService.cs
echo             _unitOfWork.Product.ToggleStatus(id); >> Services\Implementations\ProductService.cs
echo             _unitOfWork.Save(); >> Services\Implementations\ProductService.cs
echo         } >> Services\Implementations\ProductService.cs
echo. >> Services\Implementations\ProductService.cs
echo         public void DeleteProduct(int id) >> Services\Implementations\ProductService.cs
echo         { >> Services\Implementations\ProductService.cs
echo             _unitOfWork.Product.Delete(id); >> Services\Implementations\ProductService.cs
echo             _unitOfWork.Save(); >> Services\Implementations\ProductService.cs
echo         } >> Services\Implementations\ProductService.cs
echo     } >> Services\Implementations\ProductService.cs
echo } >> Services\Implementations\ProductService.cs

echo Dang cap nhat Program.cs...
echo using HealthFit.Services.Interfaces; > Program.cs.new
echo using HealthFit.Services.Implementations; >> Program.cs.new
echo using Microsoft.Extensions.DependencyInjection; >> Program.cs.new
echo. >> Program.cs.new
echo var builder = WebApplication.CreateBuilder(args); >> Program.cs.new
echo. >> Program.cs.new
echo // Add services to the container. >> Program.cs.new
echo builder.Services.AddControllersWithViews(); >> Program.cs.new
echo builder.Services.AddRazorPages(); >> Program.cs.new
echo builder.Services.AddScoped^<IUnitOfWork, UnitOfWork^>(); >> Program.cs.new
echo builder.Services.AddScoped^<IProductService, ProductService^>(); >> Program.cs.new
echo. >> Program.cs.new
echo var app = builder.Build(); >> Program.cs.new
echo. >> Program.cs.new
echo // Configure the HTTP request pipeline. >> Program.cs.new
echo if (!app.Environment.IsDevelopment()) >> Program.cs.new
echo { >> Program.cs.new
echo     app.UseSwagger(); >> Program.cs.new
echo     app.UseSwaggerUI(); >> Program.cs.new
echo } >> Program.cs.new
echo. >> Program.cs.new
echo app.UseHttpsRedirection(); >> Program.cs.new
echo app.UseStaticFiles(); >> Program.cs.new
echo app.UseRouting(); >> Program.cs.new
echo app.UseAuthorization(); >> Program.cs.new
echo. >> Program.cs.new
echo app.MapControllerRoute( >> Program.cs.new
echo     name: "areas", >> Program.cs.new
echo     pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); >> Program.cs.new
echo. >> Program.cs.new
echo app.MapControllerRoute( >> Program.cs.new
echo     name: "default", >> Program.cs.new
echo     pattern: "{controller=Home}/{action=Index}/{id?}"); >> Program.cs.new
echo. >> Program.cs.new
echo app.MapRazorPages(); >> Program.cs.new
echo. >> Program.cs.new
echo app.Run(); >> Program.cs.new

move /y Program.cs.new Program.cs

echo Hoan thanh!
pause 