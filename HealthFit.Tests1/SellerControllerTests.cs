using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthFit.Areas.Seller.Controllers;
using HealthFit.Models.Models;
using Microsoft.EntityFrameworkCore;
using HealthFit.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using HealthFit.Models;
using System.Diagnostics.CodeAnalysis;
namespace HealthFit.Tests1
{
    [TestFixture]
    public class SellerControllerTests
    {
        [SuppressMessage("Usage", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method", Justification = "Disposed in TearDown.")]
        private SellerController _controller;
        [SuppressMessage("Usage", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method", Justification = "Moq Mocks do not require Dispose.")]
        private Mock<HealthyShopContext> _mockContext;
        [SuppressMessage("Usage", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method", Justification = "Moq Mocks do not require Dispose.")]
        private Mock<UserManager<User>> _mockUserManager;

        [SetUp]
        public void Setup()
        {
            // Tạo dữ liệu mẫu
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Sữa chua", IsActive = "Active", Price = 10000 },
                new Product { ProductId = 2, Name = "Bánh mì", IsActive = "Inactive", Price = 5000 },
                new Product { ProductId = 3, Name = "Sữa tươi", IsActive = "Active", Price = 15000 }
            }.AsQueryable();

            // Mock DbSet<Product>
            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            // Mock DbContext
            _mockContext = new Mock<HealthyShopContext>();
            _mockContext.Setup(c => c.Products).Returns(mockSet.Object);

            // Mock UserManager<User>
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            // Khởi tạo controller với đủ tham số
            _controller = new SellerController(_mockUserManager.Object, _mockContext.Object);
        }

        [Test]
        public async Task ProductList_FilterByIsActive_ReturnsCorrectProducts()
        {
            // Arrange
            string filter = "Active";

            // Act
            var result = await _controller.ProductList(null, filter, null, null) as ViewResult;
            var model = result.Model as List<Product>;

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count); // "Sữa chua" và "Sữa tươi"
            Assert.IsTrue(model.All(p => p.IsActive == "Active"));
        }

        [TearDown]
        public void TearDown()
        {
            (_controller as IDisposable)?.Dispose();
        }
    }
}

