namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICartItemRepository CartItem { get; }
        IUserRepository User { get; }
        IProductRepository Product { get; }
        IVnpayRepository Payment { get; }
        IProductCategoryRepository ProductCategory { get; }
        IOrderMealPlanDetailRepository OrderMealPlanDetail { get; }
        IOrderRepository Order { get; }
        IOrderProductDetailRepository OrderProductDetail { get; }

        IMealPlanRepository MealPlan { get; }

        ICustomerProfileRepository CustomerProfile { get; }

        void Save();
    }
}
