using System.Linq.Expressions;
using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.Constants;
using HealthFit.Models.DTO.Product;
using Microsoft.EntityFrameworkCore;

namespace HealthFit.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly HealthyShopContext _dbContext;

        public ProductRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(Product obj)
        {
            _dbContext.Products.Update(obj);
        }

        public async Task<Product?> GetFirstOrDefaultAsync(Expression<Func<Product, bool>> filter)
        {
            return await _dbContext.Products.FirstOrDefaultAsync(filter);
        }

        public async Task<Product?> FindByIdAsync(int id)
        {
            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsFilteredAsync(string? showHidden)
        {
            var query = _dbContext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(showHidden))
            {
                query = query.Where(p => p.IsActive != null && p.IsActive.Equals(showHidden, StringComparison.OrdinalIgnoreCase));
            }

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<Product>> SearchAsync(string search, string? showHidden, string? caloRange, string? priceRange, int? categoryId)
        {
            var query = _dbContext.Products.AsQueryable();

            // Lọc trạng thái
            if (!string.IsNullOrWhiteSpace(showHidden))
            {
                query = query.Where(p => p.IsActive == showHidden);
            }


            // Lọc calo
            if (!string.IsNullOrEmpty(caloRange))
            {
                switch (caloRange)
                {
                    case "0-100":
                        query = query.Where(p => p.Calo >= 0 && p.Calo <= 100);
                        break;
                    case "100-200":
                        query = query.Where(p => p.Calo > 100 && p.Calo <= 200);
                        break;
                    case "200-300":
                        query = query.Where(p => p.Calo > 200 && p.Calo <= 300);
                        break;
                    case "300-400":
                        query = query.Where(p => p.Calo > 300 && p.Calo <= 400);
                        break;
                    case "400-500":
                        query = query.Where(p => p.Calo > 400 && p.Calo <= 500);
                        break;
                    case "500+":
                        query = query.Where(p => p.Calo > 500);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "0-20000":
                        query = query.Where(p => p.Price <= 20000);
                        break;
                    case "20000-40000":
                        query = query.Where(p => p.Price > 20000 && p.Price <= 40000);
                        break;
                    case "40000-60000":
                        query = query.Where(p => p.Price > 40000 && p.Price <= 60000);
                        break;
                    case "60001+":
                        query = query.Where(p => p.Price > 60000);
                        break;
                }
            }
            if (categoryId != null)
            {
                query = query.Where(p => p.ProductCategoryMappings
                   .Any(m => m.CategoryId == categoryId.Value));
            }

            // Lọc tên/mô tả
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{search}%") ||
                    (p.Description != null && EF.Functions.Like(p.Description, $"%{search}%")) ||
                    (p.Ingredients != null && EF.Functions.Like(p.Ingredients, $"%{search}%")));
            }

            return await query.ToListAsync();
        }
        // Cập nhật sản phẩm 
        public async Task UpdateAsync(Product product)
        {
            _dbContext.Products.Update(product);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
            }
        }

        public async Task<ProductDetailDto?> GetProductDetailAsync(int id)
        {
            return await _dbContext.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDetailDto
                {
                    ProductId = p.ProductId,
                    QuantityInStock = p.Quantity,
                    Calo = p.Calo,
                    CreatedByName = p.CreatedByNavigation.FullName,
                    QuantitySold = _dbContext.OrderProductDetails
                                          .Where(od => od.ProductId == id)
                                          .Sum(od => (int)od.Quantity),
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    Description = p.Description,
                    Name = p.Name
                })
                .FirstOrDefaultAsync();

        }

        public async Task<List<ProductCategory>> GetProductCategories()
        {
            return await _dbContext.ProductCategories.ToListAsync();
        }

        public async Task UpdateStatusAsync(int productId, string newStatus)
        {
            if (!ProductStatusConstants.All.Contains(newStatus))
                throw new ArgumentException("Invalid status");

            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
                throw new Exception("Product not found");

            product.IsActive = newStatus;
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}
