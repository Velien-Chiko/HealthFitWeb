using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;

namespace HealthFit.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HealthyShopContext _dbContext;
        public ICartItemRepository CartItem { get; private set; }
        public IUserRepository User { get; private set; }
        public IProductCategoryRepository ProductCategory { get; private set; }
        public IProductRepository Product { get; private set; }
        public IVnpayRepository Payment { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderProductDetailRepository OrderProductDetail { get; private set; }
        public IMealPlanRepository MealPlan { get; private set; }
        public IOrderMealPlanDetailRepository OrderMealPlanDetail { get; private set; }
        public ICustomerProfileRepository CustomerProfile { get; private set; }

        public UnitOfWork(HealthyShopContext dbContext)
        {
            _dbContext = dbContext;
            CartItem = new CartItemRepository(_dbContext);
            User = new UserRepository(_dbContext);
            Product = new ProductRepository(_dbContext);
            Order = new OrderRepository(_dbContext);
            MealPlan = new MealPlanRepository(_dbContext);
            CustomerProfile = new CustomerProfileRepository(_dbContext);
            ProductCategory = new ProductCategoryRepository(_dbContext);
            OrderMealPlanDetail = new OrderMealPlanDetailRepository(_dbContext);
            Payment = new VnpayRepository(_dbContext);
            OrderProductDetail = new OrderProductDetailRepository(_dbContext);

        }


        public void Save()
        {
            _dbContext.SaveChanges();
        }
        // Dispose method can be added if needed for resource cleanup

    }
}
