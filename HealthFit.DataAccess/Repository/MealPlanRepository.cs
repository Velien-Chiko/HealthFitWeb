using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.DTO.MealPlan;
using Microsoft.EntityFrameworkCore;

namespace HealthFit.DataAccess.Repository
{
    public class MealPlanRepository : Repository<Models.MealPlanDetail>, IMealPlanRepository
    {
        private readonly HealthyShopContext _dbContext;

        public MealPlanRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> CalculateSavingsForMealPlanAsync(int mealPlanId)
        {
            var mealPlan = await _dbContext.MealPlanDetails
                .Where(m => m.MealPlanDetailId == mealPlanId)
                .Select(m => new
                {
                    MealPlanPrice = m.Price,
                    Products = m.MealPlanProductDetails.Select(mp => new
                    {
                        ProductPrice = mp.Product.Price,
                        Quantity = mp.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (mealPlan == null) return 0;


            decimal totalRetailPrice = mealPlan.Products
                .Sum(p => p.ProductPrice * p.Quantity);


            decimal savings = totalRetailPrice - mealPlan.MealPlanPrice;


            return savings > 0 ? savings : 0;
        }

        public async Task<MealPlanDetail?> FindByIdAsync(int id)
        {
            return await _dbContext.MealPlanDetails.FindAsync(id);
        }

        public async Task<IEnumerable<MealPlanDetail>> GetAllMealPlanDetailsAsync()
        {
            return await _dbContext.MealPlanDetails.ToListAsync();

        }

        public async Task<string?> GetIsApprovedAsync(int id)
        {
            var mealDetail = await _dbContext.MealPlanDetails.FindAsync(id);
            return mealDetail?.IsApproved;
        }

        public async Task<IEnumerable<MealPlanDetail>> GetMealPlanFilteredAsync(string? isApproved)
        {
            var query = _dbContext.MealPlanDetails.AsQueryable();
            if (!string.IsNullOrEmpty(isApproved))
            {
                query = query.Where(m => m.IsApproved == isApproved);
            }
            return await query.ToListAsync();
        }

        public async Task<MealPlanDto> GetMealPlanWithProductsAsync(int mealPlanId)
        {
            // Lấy dữ liệu MealPlan + các sản phẩm liên quan
            var mealPlan = await _dbContext.MealPlanDetails
                .Where(m => m.MealPlanDetailId == mealPlanId)
                .Select(m => new
                {
                    m.MealPlanDetailId,
                    m.PlanDescription,
                    m.ImageUrl,
                    m.Price,
                    m.Bmirange,
                    Products = m.MealPlanProductDetails.Select(mp => new
                    {
                        mp.ProductId,
                        ProductName = mp.Product.Name,
                        Calo = mp.Product.Calo,
                        ProductStock = mp.Product.Quantity,
                        NeededPerCombo = mp.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (mealPlan == null)
                return null;

            // Tính tổng số lượng đã bán (ngoài EF để tách biệt logic)
            var quantitySold = await _dbContext.OrderMealPlanDetails
                .Where(od => od.MealPlanDetailId == mealPlanId)
                .SumAsync(od => (int?)od.Quantity) ?? 0;

            // Tính số lượng combo còn lại dựa trên tồn kho
            var quantityInStock = mealPlan.Products
                .Where(p => p.NeededPerCombo > 0)
                .Select(p => p.ProductStock / p.NeededPerCombo)
                .DefaultIfEmpty(0)
                .Min();

            // Map sang DTO để trả về
            return new MealPlanDto
            {
                MealPlanId = mealPlan.MealPlanDetailId,
                MealPlanName = mealPlan.PlanDescription,
                ImageUrl = mealPlan.ImageUrl,
                Price = mealPlan.Price,
                Bmirange = mealPlan.Bmirange,
                QuantitySold = quantitySold,
                QuantityInStock = quantityInStock,
                Products = mealPlan.Products.Select(p => new MealPlanProductDto
                {
                    ProductID = p.ProductId,
                    ProductName = p.ProductName,
                    Calo = p.Calo,
                    Quantity = p.NeededPerCombo
                }).ToList()
            };
        }



        public async Task<IEnumerable<MealPlanDetail>> SearchMealPlanDetailsAsync(string keyword, string? BMI, string? isApproved, string? priceRange)
        {
            var query = _dbContext.MealPlanDetails
                .Include(m => m.MealPlanProductDetails)
                .ThenInclude(m => m.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(isApproved))
            {
                query = query.Where(m => m.IsApproved == isApproved);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(m =>
                                    m.PlanDescription != null &&
                                    EF.Functions.Like(m.PlanDescription, $"%{keyword}%"));

            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                if (priceRange.Contains('-'))
                {
                    var parts = priceRange.Split('-');
                    if (int.TryParse(parts[0], out int minPrice) && int.TryParse(parts[1], out int maxPrice))
                    {
                        query = query.Where(m => m.Price >= minPrice && m.Price <= maxPrice);
                    }
                }
                else if (priceRange.StartsWith(">"))
                {
                    if (int.TryParse(priceRange.Substring(1), out int minPrice))
                    {
                        query = query.Where(m => m.Price > minPrice);
                    }
                }
                else if (priceRange.StartsWith("<"))
                {
                    if (int.TryParse(priceRange.Substring(1), out int maxPrice))
                    {
                        query = query.Where(m => m.Price < maxPrice);
                    }
                }
            }

            if (!string.IsNullOrEmpty(BMI) && float.TryParse(BMI, out float userBmi))
            {
                var results = await query.ToListAsync();
                return results.Where(m =>
                {
                    var range = m.Bmirange?.Split('-');
                    if (range?.Length == 2 &&
                        float.TryParse(range[0], out float min) &&
                        float.TryParse(range[1], out float max))
                    {
                        return userBmi >= min && userBmi <= max;
                    }
                    return false;
                });
            }

            return await query.ToListAsync();
        }

        public async Task UpdateMealPlanInfoAsync(int mealPlanId, decimal? newPrice, string? newStatus, string? newImageUrl)
        {
            var mealPlan = await _dbContext.MealPlanDetails.FindAsync(mealPlanId);
            if (mealPlan == null)
                throw new KeyNotFoundException($"Không tìm thấy MealPlan với ID: {mealPlanId}");

            if (newPrice.HasValue)
                mealPlan.Price = newPrice.Value;

            if (!string.IsNullOrWhiteSpace(newStatus))
                mealPlan.IsApproved = newStatus;

            if (!string.IsNullOrWhiteSpace(newImageUrl))
                mealPlan.ImageUrl = newImageUrl;

            _dbContext.MealPlanDetails.Update(mealPlan);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateStatusMealPlanAsync(int mealPlanId, string newStatus)
        {
            var mealPlan = await _dbContext.MealPlanDetails.FindAsync(mealPlanId);
            if (mealPlan == null)
                throw new KeyNotFoundException($"Không tìm thấy MealPlan với ID: {mealPlanId}");

            // Không validate newStatus ở đây
            mealPlan.IsApproved = newStatus;
            _dbContext.MealPlanDetails.Update(mealPlan);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(MealPlanDetail mealPlan)
        {
            var existing = await _dbContext.MealPlanDetails.FindAsync(mealPlan.MealPlanDetailId);
            if (existing == null)
                throw new Exception("Không tìm thấy kế hoạch bữa ăn.");


            existing.Price = mealPlan.Price;
            existing.IsApproved = mealPlan.IsApproved;
            existing.ImageUrl = mealPlan.ImageUrl;


            _dbContext.MealPlanDetails.Update(existing);
        }
        public void Update(CartItem obj)
        {
            _dbContext.CartItems.Update(obj);
        }


    }
}
