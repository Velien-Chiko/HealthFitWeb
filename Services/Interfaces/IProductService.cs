using HealthFit.Models;
using HealthFit.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace HealthFit.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> SearchProducts(string searchString);
        Task<Product?> GetProductById(int id);
        Task<bool> UpdateProduct(Product product, IFormFile? imageFile);
        Task<bool> ToggleProductStatus(int id);
        Task<bool> DeleteProduct(int id);
    }
} 