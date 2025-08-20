using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.Models;

namespace HealthFit.DataAccess.Repository
{
    public class OrderMealPlanDetailRepository : Repository<OrderMealPlanDetail>, IOrderMealPlanDetailRepository
    {
        private readonly HealthyShopContext _dbContext;
        public OrderMealPlanDetailRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(OrderMealPlanDetail obj)
        {
            _dbContext.OrderMealPlanDetails.Update(obj);
        }

    }
}
