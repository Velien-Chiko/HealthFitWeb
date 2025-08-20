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
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository
    {
        private readonly HealthyShopContext _dbContext;
        public CartItemRepository(HealthyShopContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public void Update(CartItem obj)
        {
            _dbContext.CartItems.Update(obj);
        }

    }
}
