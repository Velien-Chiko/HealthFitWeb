using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Services.Interfaces;

namespace HealthFit.Services.Implementations
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerProfileService> _logger;

        public CustomerProfileService(IUnitOfWork unitOfWork, ILogger<CustomerProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CustomerProfile?> GetProfileByUserIdAsync(int userId)
        {
            return await _unitOfWork.CustomerProfile.GetByUserIdAsync(userId);
        }

        public async Task<CustomerProfile?> GetProfileByUserIdWithUserAsync(int userId)
        {
            return await _unitOfWork.CustomerProfile.GetByUserIdWithUserAsync(userId);
        }

        public async Task<CustomerProfile> CreateProfileAsync(CustomerProfile profile)
        {
            return await _unitOfWork.CustomerProfile.CreateAsync(profile);
        }

        public async Task<CustomerProfile> UpdateProfileAsync(CustomerProfile profile, int userId)
        {
            var existingProfile = await GetProfileByUserIdAsync(userId);
            if (existingProfile == null)
            {
                throw new Exception("Không tìm thấy profile");
            }

            // Verify that the user owns this profile
            if (existingProfile.UserId != userId)
            {
                throw new UnauthorizedAccessException("Không có quyền cập nhật profile này");
            }

            // Update only the fields that can be modified
            existingProfile.Height = profile.Height;
            existingProfile.Weight = profile.Weight;
            existingProfile.Gender = profile.Gender;
            existingProfile.Age = profile.Age;

            // Calculate BMI if height and weight are provided
            if (existingProfile.Height.HasValue && existingProfile.Weight.HasValue)
            {
                // BMI = weight (kg) / (height (m))²
                var heightInMeters = existingProfile.Height.Value / 100; // Convert cm to m
                existingProfile.Bmi = (int)(existingProfile.Weight.Value / (heightInMeters * heightInMeters));
            }

            return await _unitOfWork.CustomerProfile.UpdateAsync(existingProfile);
        }

        public async Task<bool> ProfileExistsAsync(int profileId)
        {
            return await _unitOfWork.CustomerProfile.ExistsAsync(profileId);
        }

        public async Task<CustomerProfile> GetOrCreateProfileAsync(int userId)
        {
            var profile = await GetProfileByUserIdWithUserAsync(userId);

            if (profile == null)
            {
                // Create new profile if it doesn't exist
                profile = new CustomerProfile
                {
                    UserId = userId
                };
                profile = await CreateProfileAsync(profile);

                // Reload profile with user information
                profile = await GetProfileByUserIdWithUserAsync(userId);
            }

            return profile;
        }
    }
}