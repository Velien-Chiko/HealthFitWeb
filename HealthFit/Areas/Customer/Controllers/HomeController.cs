using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using HealthFit.DataAccess.Repository;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.DTO.MealPlan;
using HealthFit.Models.ViewModels;
using HealthFit.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HealthFit.Areas.Customer.Controllers
{

    [Area("Customer")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        private string GetUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }

        public IActionResult Index()
        {
            List<ProductCategory> productCategories = _unitOfWork.ProductCategory.GetAll().ToList();
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "ProductCategoryMappings.Category").ToList();
            return View(products);
        }
        public IActionResult Order()
        {
            string? userIdStr = GetUserId();

            ViewBag.HasAccount = !string.IsNullOrEmpty(userIdStr);

            IEnumerable<Order> orders = Enumerable.Empty<Order>();

            if (ViewBag.HasAccount)
            {
                int userId = int.Parse(userIdStr);
                orders = _unitOfWork.Order.GetAll(
                    filter: o => o.UserId == userId,
                    includeProperties: "OrderProductDetails,OrderMealPlanDetails"
                ).ToList();

                // Nếu chưa có navigation trong includeProperties, cần thêm hoặc xử lý thêm ở repo
            }

            return View("Order", orders);
        }
        public IActionResult OrderDetail(int id)
        {
            // Lấy đơn hàng theo id kèm navigation properties cần thiết
            var order = _unitOfWork.Order.Get(
                o => o.OrderId == id,
                includeProperties: "User,OrderProductDetails.Product,OrderMealPlanDetails.MealPlanDetail"
            );

            if (order == null)
            {
                return NotFound(); // Hoặc trả về view lỗi riêng tùy ý
            }

            // Chuyển dữ liệu về một ViewModel để View dễ dùng
            var viewModel = new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                City = order.City,
                Country = order.Country,
                Email = order.Email,
                PaymentStatus = order.PaymentStatus,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderProducts = order.OrderProductDetails.Select(opd => new OrderProductItem
                {
                    ProductName = opd.Product?.Name ?? "Sản phẩm",
                    Quantity = opd.Quantity,
                    UnitPrice = opd.UnitPrice,
                    SubTotal = opd.Subtotal
                }).ToList(),

                OrderMealPlans = order.OrderMealPlanDetails.Select(omp => new OrderMealPlanItem
                {
                    MealPlanName = omp.MealPlanDetail?.PlanDescription ?? "Combo",
                    Quantity = omp.Quantity,
                    UnitPrice = omp.UnitPrice,
                    SubTotal = omp.UnitPrice * omp.Quantity
                }).ToList()
            };

            return View(viewModel);
        }






        public IActionResult Details(int id)
        {
            CartItem cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.ProductId == id, includeProperties: "ProductCategoryMappings.Category"),
                ProductId = id,
                Quantity = 1 // Mặc định khi mở chi tiết sản phẩm
            };
            return View(cart);
        }

        [HttpPost]
        public IActionResult Details(CartItem cartItem)
        {
            string userId = GetUserId();

            if (userId != null)
            {
                // Người dùng đã đăng nhập → lưu vào database
                cartItem.UserId = int.Parse(userId);
                var cartFromDb = _unitOfWork.CartItem.Get(u => u.UserId == cartItem.UserId && u.ProductId == cartItem.ProductId);
                if (cartFromDb != null)
                {
                    cartFromDb.Quantity += cartItem.Quantity;
                    _unitOfWork.CartItem.Update(cartFromDb);
                }
                else
                {
                    _unitOfWork.CartItem.Add(cartItem);
                }
                _unitOfWork.Save();
            }
            else
            {
                // Người dùng chưa đăng nhập → lưu vào Session
                var sessionCart = HttpContext.Session.GetString("CartSession");
                List<CartItem> cartItems = string.IsNullOrEmpty(sessionCart)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(sessionCart);

                var existingItem = cartItems.FirstOrDefault(c => c.ProductId == cartItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += cartItem.Quantity;
                }
                else
                {
                    // Lấy thông tin sản phẩm để hiển thị lại ở Index
                    var product = _unitOfWork.Product.Get(p => p.ProductId == cartItem.ProductId, includeProperties: "ProductCategoryMappings.Category");
                    cartItem.Product = product;
                    cartItems.Add(cartItem);
                }

                HttpContext.Session.SetString("CartSession", JsonSerializer.Serialize(cartItems));
            }

            return RedirectToAction(nameof(Index));
        }
       
        

    }

}