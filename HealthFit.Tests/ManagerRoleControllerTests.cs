using NUnit.Framework;
using Moq;
using HealthFit.Areas.SystemAdmin.Controllers;
using HealthFit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HealthFit.Tests
{
    [TestFixture]
    public class ManagerRoleControllerTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<RoleManager<IdentityRole<int>>> _roleManagerMock;
        private ManagerRoleController _controller;

        [SetUp]
        public void Setup()
        {
            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            var roleStore = new Mock<IRoleStore<IdentityRole<int>>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(roleStore.Object, null, null, null, null);
            _controller = new ManagerRoleController(null, _userManagerMock.Object);
        }

        [Test]
        public async Task Create_ValidInput_ReturnsRedirectToSystemAdmin()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123";
            var fullName = "Test User";
            var role = "Admin";
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), role))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Create(email, password, fullName, role);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("SystemAdmin", redirect.ActionName);
        }

        [Test]
        public async Task Update_ValidInput_ReturnsRedirectToSystemAdmin()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            _userManagerMock.Setup(x => x.Users).Returns(new List<User> { user }.AsQueryable());
            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Manager")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "token", "NewPass@123")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Update(1, user.Email, "NewPass@123", "New Name", "Manager", "addr", "city", "country", "male");

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("SystemAdmin", redirect.ActionName);
        }
    }
} 