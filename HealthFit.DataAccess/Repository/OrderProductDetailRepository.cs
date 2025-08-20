using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
namespace HealthFit.DataAccess.Repository
{
    public class OrderProductDetailRepository : Repository<OrderProductDetail>, IOrderProductDetailRepository
    {
        private readonly HealthyShopContext _dbContext;
        public OrderProductDetailRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(OrderProductDetail obj)
        {
            _dbContext.OrderProductDetails.Update(obj);
        }
    }
}