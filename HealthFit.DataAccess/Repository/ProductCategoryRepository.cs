using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;

namespace HealthFit.DataAccess.Repository
{
    public class ProductCategoryRepository : Repository<ProductCategory>, IProductCategoryRepository
    {
        private readonly HealthyShopContext _dbContext;
        public ProductCategoryRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(ProductCategory obj)
        {
            _dbContext.ProductCategories.Update(obj);
        }
    }
}
