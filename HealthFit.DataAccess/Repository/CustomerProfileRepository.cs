using HealthFit.DataAccess.Data;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthFit.DataAccess.Repository
{
    public class CustomerProfileRepository : Repository<CustomerProfile>, ICustomerProfileRepository
    {
        private readonly HealthyShopContext _context;

        public CustomerProfileRepository(HealthyShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CustomerProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.CustomerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<CustomerProfile?> GetByUserIdWithUserAsync(int userId)
        {
            return await _context.CustomerProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<bool> ExistsAsync(int profileId)
        {
            return await _context.CustomerProfiles.AnyAsync(e => e.ProfileId == profileId);
        }

        public async Task<CustomerProfile> CreateAsync(CustomerProfile profile)
        {
            _context.CustomerProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<CustomerProfile> UpdateAsync(CustomerProfile profile)
        {
            _context.Update(profile);
            await _context.SaveChangesAsync();
            return profile;
        }
    }
} 