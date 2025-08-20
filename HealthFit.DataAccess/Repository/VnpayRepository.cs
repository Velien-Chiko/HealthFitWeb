using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models.Models;

namespace HealthFit.DataAccess.Repository
{
    public class VnpayRepository : Repository<Payment>, IVnpayRepository
    {
        private readonly HealthyShopContext _dbContext;
        public VnpayRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(Payment obj)
        {
            _dbContext.Payments.Update(obj);
        }
    }
}
