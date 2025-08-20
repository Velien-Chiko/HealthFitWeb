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
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly HealthyShopContext _dbContext;
        public UserRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(User obj)
        {
            _dbContext.Users.Update(obj);
        }
    }
}
