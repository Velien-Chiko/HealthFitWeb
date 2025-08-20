using HealthFit.Models;

namespace HealthFit.DataAccess.Repository.IRepository
{
    public interface IOrderProductDetailRepository : IRepository<OrderProductDetail>
    {
        void Update(OrderProductDetail obj);
    }
}