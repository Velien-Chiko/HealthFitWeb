using System.Security.Claims;
using System.Text.Json;
using HealthFit.DataAccess.Repository.IRepository;
using HealthFit.Models;
using HealthFit.Models.Models;
using HealthFit.Models.Models.VNPay;
using HealthFit.Models.ViewModels;
using HealthFit.Services;
using HealthFit.Services.Interfaces;
using HealthFit.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HealthFit.Areas.Customer.Controllers
{
    [Area("customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public CartItemVM CartItemVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, IVnPayService vnPayService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _vnPayService = vnPayService;
            _configuration = configuration;
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
            string userId = GetUserId();
            IEnumerable<CartItem> cartItems;

             if (userId != null)
            {
                // Người dùng đã đăng nhập, lấy cart cùng các navigation property
                cartItems = _unitOfWork.CartItem.GetAll(
                    u => u.UserId.ToString() == userId,
                    includeProperties: "Product,MealPlanDetail,MealPlanDetail.MealPlanProductDetails.Product"
                );
            }
            else
            {
                // Người dùng chưa đăng nhập, lấy từ session
                var sessionCart = HttpContext.Session.GetString("CartSession");
                cartItems = string.IsNullOrEmpty(sessionCart)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(sessionCart);

                // Lấy dữ liệu chi tiết Product hoặc MealPlan theo Id (đồng bộ)
                foreach (var item in cartItems)
                {
                    if (item.ProductId.HasValue)
                    {
                        // Giả sử _unitOfWork.Product.Get trả về object đồng bộ
                        item.Product = _unitOfWork.Product.Get(p => p.ProductId == item.ProductId.Value);
                    }
                    else if (item.MealPlanDetailId.HasValue)
                    {
                        item.MealPlanDetail = _unitOfWork.MealPlan.Get(
                            m => m.MealPlanDetailId == item.MealPlanDetailId.Value,
                            includeProperties: "MealPlanProductDetails.Product"
                        );
                    }
                }
            }
            var cartDisplayList = new List<CartItemDisplayDTO>();
            decimal total = 0;

            foreach (var item in cartItems)
            {
                if (item.ProductId.HasValue && item.Product != null)
                {
                    cartDisplayList.Add(new CartItemDisplayDTO
                    {
                        CartItemId = item.CartItemId,
                        ProductId = item.ProductId,
                        Name = item.Product.Name,
                        UnitPrice = item.Product.Price,
                        Quantity = item.Quantity,
                        MaxQuantity = item.Product.Quantity,
                        ImageUrl = item.Product.ImageUrl,
                        Description = item.Product.Description,
                    });

                    total += item.Product.Price * item.Quantity;
                }
                else if (item.MealPlanDetailId.HasValue && item.MealPlanDetail != null)
                {
                    var mealPlan = item.MealPlanDetail;

                    int quantityInStock = mealPlan.MealPlanProductDetails
                        .Where(mp => mp.Quantity > 0)
                        .Select(mp => mp.Product.Quantity / mp.Quantity)
                        .DefaultIfEmpty(0)
                        .Min();

                    cartDisplayList.Add(new CartItemDisplayDTO
                    {
                        CartItemId = item.CartItemId,
                        MealPlanDetailId = mealPlan.MealPlanDetailId,
                        Name = mealPlan.PlanDescription,
                        UnitPrice = mealPlan.Price,
                        Quantity = item.Quantity,
                        MaxQuantity = quantityInStock,
                        ImageUrl = mealPlan.ImageUrl,
                    });

                    total += mealPlan.Price * item.Quantity;
                }
            }

            var vm = new CartItemVM
            {
                CartItemList = cartDisplayList,
                Order = new OrderInputDTO
                {
                    TotalAmount = total
                }
            };

            return View(vm);
        }
        [HttpPost]
        public IActionResult UpdateCart(CartItemVM model)
        {
            string userId = GetUserId();

            if (model == null || model.CartItemList == null)
            {
                TempData["ErrorMessage"] = "Không có sản phẩm cần cập nhật!";
                return RedirectToAction("Index");
            }

            // Người dùng đã login (lưu ở database)
            if (!string.IsNullOrEmpty(userId))
            {
                int intUserId = int.Parse(userId);
                foreach (var item in model.CartItemList)
                {
                    var cartItem = _unitOfWork.CartItem.Get(c =>
                        c.UserId == intUserId &&
                        c.CartItemId == item.CartItemId);

                    if (cartItem != null)
                    {
                        // Check tồn kho
                        int maxQty;
                        if (cartItem.ProductId.HasValue)
                            maxQty = _unitOfWork.Product.Get(p => p.ProductId == cartItem.ProductId.Value).Quantity;
                        else if (cartItem.MealPlanDetailId.HasValue)
                        {
                            var mealPlan = _unitOfWork.MealPlan.Get(
                                m => m.MealPlanDetailId == cartItem.MealPlanDetailId.Value,
                                includeProperties: "MealPlanProductDetails.Product"
                            );
                            maxQty = mealPlan.MealPlanProductDetails
                                .Where(mp => mp.Quantity > 0)
                                .Select(mp => mp.Product.Quantity / mp.Quantity)
                                .DefaultIfEmpty(0)
                                .Min();
                        }
                        else maxQty = 999;

                        cartItem.Quantity = Math.Max(1, Math.Min(item.Quantity, maxQty));
                        cartItem.AddedDate = DateTime.Now;
                        _unitOfWork.CartItem.Update(cartItem);
                    }
                }
                _unitOfWork.Save();
            }
            // Người dùng chưa login (session)
            else
            {
                var sessionCart = HttpContext.Session.GetString("CartSession");
                var cartItems = string.IsNullOrEmpty(sessionCart)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(sessionCart);

                foreach (var item in model.CartItemList)
                {
                    var cartItem = cartItems.FirstOrDefault(c => c.CartItemId == item.CartItemId);
                    if (cartItem != null)
                    {
                        int maxQty;
                        if (cartItem.ProductId.HasValue)
                            maxQty = _unitOfWork.Product.Get(p => p.ProductId == cartItem.ProductId.Value).Quantity;
                        else if (cartItem.MealPlanDetailId.HasValue)
                        {
                            var mealPlan = _unitOfWork.MealPlan.Get(
                                m => m.MealPlanDetailId == cartItem.MealPlanDetailId.Value,
                                includeProperties: "MealPlanProductDetails.Product"
                            );
                            maxQty = mealPlan.MealPlanProductDetails
                                .Where(mp => mp.Quantity > 0)
                                .Select(mp => mp.Product.Quantity / mp.Quantity)
                                .DefaultIfEmpty(0)
                                .Min();
                        }
                        else maxQty = 999;

                        cartItem.Quantity = Math.Max(1, Math.Min(item.Quantity, maxQty));
                        cartItem.AddedDate = DateTime.Now;
                    }
                }
                HttpContext.Session.SetString("CartSession", JsonSerializer.Serialize(cartItems));
            }

            TempData["SuccessMessage"] = "Cập nhật giỏ hàng thành công!";
            return RedirectToAction("Index");
        }


        public IActionResult Remove(int cartId)
        {
            string userId = GetUserId();

            if (userId != null)
            {
                var cartFromDb = _unitOfWork.CartItem.Get(u => u.CartItemId == cartId);
                _unitOfWork.CartItem.Remove(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                var sessionCart = HttpContext.Session.GetString("CartSession");
                var cartItems = string.IsNullOrEmpty(sessionCart) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(sessionCart);
                var item = cartItems.FirstOrDefault(c => c.CartItemId == cartId);
                if (item != null)
                {
                    cartItems.Remove(item);
                    HttpContext.Session.SetString("CartSession", JsonSerializer.Serialize(cartItems));
                }
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Summary()
        {
            string userId = GetUserId();
            var cartItems = LoadCartItems(userId, out User? user);

            decimal totalAmount = 0m;
            var cartDisplayList = new List<CartItemDisplayDTO>();

            foreach (var item in cartItems)
            {
                if (item.ProductId.HasValue && item.Product != null)
                {
                    cartDisplayList.Add(new CartItemDisplayDTO
                    {
                        CartItemId = item.CartItemId,
                        ProductId = item.ProductId,
                        Name = item.Product.Name,
                        UnitPrice = item.Product.Price,
                        Quantity = item.Quantity,
                        MaxQuantity = item.Product.Quantity,
                        ImageUrl = item.Product.ImageUrl,
                        Description = item.Product.Description
                    });

                    totalAmount += item.Product.Price * item.Quantity;
                }
                else if (item.MealPlanDetailId.HasValue && item.MealPlanDetail != null)
                {
                    var mealPlan = item.MealPlanDetail;

                    int quantityInStock = mealPlan.MealPlanProductDetails != null && mealPlan.MealPlanProductDetails.Any()
                        ? mealPlan.MealPlanProductDetails
                            .Where(mp => mp.Quantity > 0)
                            .Select(mp => mp.Product.Quantity / mp.Quantity)
                            .DefaultIfEmpty(0)
                            .Min()
                        : 0;

                    cartDisplayList.Add(new CartItemDisplayDTO
                    {
                        CartItemId = item.CartItemId,
                        MealPlanDetailId = mealPlan.MealPlanDetailId,
                        Name = mealPlan.PlanDescription,
                        UnitPrice = mealPlan.Price,
                        Quantity = item.Quantity,
                        MaxQuantity = quantityInStock,
                        ImageUrl = mealPlan.ImageUrl
                    });

                    totalAmount += mealPlan.Price * item.Quantity;
                }
                else
                {
                    // Optional: log or handle unexpected item with missing navigation
                }
            }

            var vm = new CartItemVM
            {
                CartItemList = cartDisplayList,
                Order = new OrderInputDTO
                {
                    PhoneNumber = user?.PhoneNumber ?? string.Empty,
                    Address = user?.Address ?? string.Empty,
                    City = user?.City ?? string.Empty,
                    Country = "Việt Nam",
                    FullName = user?.FullName ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    TotalAmount = totalAmount
                }
            };
            return View(vm);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Summary(CartItemVM vm)
        {
            string? userIdStr = GetUserId();
            var cartItems = LoadCartItems(userIdStr, out _);

            ModelState.Remove(nameof(CartItemVM.CartItemList));

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Model error at '{state.Key}': {error.ErrorMessage}");
                    }
                }

                vm.CartItemList = cartItems.Select(item => new CartItemDisplayDTO
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    Name = item.Product?.Name,
                    UnitPrice = item.Product?.Price ?? 0,
                    Quantity = item.Quantity,
                    MaxQuantity = item.Product?.Quantity ?? 0,
                    ImageUrl = item.Product?.ImageUrl,
                    Description = item.Product?.Description
                }).ToList();

                return View(vm);
            }

            int? userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : (int?)null;
            decimal totalAmount = cartItems.Sum(c => (c.Product?.Price ?? c.MealPlanDetail?.Price ?? 0) * c.Quantity);

            var newOrder = new Order
            {
                UserId = userId ?? 0,
                OrderDate = DateTime.Now,
                PhoneNumber = vm.Order.PhoneNumber,
                Address = vm.Order.Address,
                City = vm.Order.City,
                Country = vm.Order.Country,
                FullName = vm.Order.FullName,
                Email = vm.Order.Email,
                PaymentStatus = SD.PaymentStatusDelayedPayment,
                Status = SD.StatusPending,
                TotalAmount = totalAmount
            };
            _unitOfWork.Order.Add(newOrder);
            _unitOfWork.Save();

            foreach (var cart in cartItems)
            {
                if (cart.ProductId.HasValue && cart.Product != null)
                {
                    _unitOfWork.OrderProductDetail.Add(new OrderProductDetail
                    {
                        OrderId = newOrder.OrderId,
                        ProductId = cart.ProductId.Value,
                        UnitPrice = cart.Product.Price,
                        Quantity = cart.Quantity,
                        Subtotal = cart.Product.Price * cart.Quantity
                    });
                }
                else if (cart.MealPlanDetailId.HasValue)
                {
                    // Sửa: luôn lấy MealPlanDetail từ DB nếu bị null
                    var mealPlanDetail = cart.MealPlanDetail 
                        ?? _unitOfWork.MealPlan.Get(m => m.MealPlanDetailId == cart.MealPlanDetailId.Value);

                    if (mealPlanDetail != null)
                    {
                        _unitOfWork.OrderMealPlanDetail.Add(new OrderMealPlanDetail
                        {
                            OrderId = newOrder.OrderId,
                            MealPlanDetailId = cart.MealPlanDetailId.Value,
                            UnitPrice = mealPlanDetail.Price,
                            Quantity = cart.Quantity
                        });
                    }
                    else
                    {
                        // Log or handle missing MealPlanDetail
                        Console.WriteLine($"MealPlanDetail not found for CartItemId={cart.CartItemId}, MealPlanDetailId={cart.MealPlanDetailId}");
                        continue;
                    }
                }
                else
                {
                    // Optional: log warning for unexpected cart item missing navigation props
                }
            }
            _unitOfWork.Save();

            var payModel = new PaymentInformationModel
            {
                OrderId = newOrder.OrderId,
                Name = newOrder.FullName,
                Amount = newOrder.TotalAmount,
                OrderDescription = "Thanh toán đơn hàng qua VNPAY",
                OrderType = "other"
            };
            string url = _vnPayService.CreatePaymentUrl(payModel, HttpContext);
            return Redirect(url);
        }


        // Load giỏ hàng (đã include navigation, improved null safety)
        private List<CartItem> LoadCartItems(string? userId, out User? user)
        {
            user = null;
            List<CartItem> items = new List<CartItem>();

            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
            {
                items = _unitOfWork.CartItem.GetAll(
                    c => c.UserId == userIdInt,
                    includeProperties: "Product,MealPlanDetail,MealPlanDetail.MealPlanProductDetails.Product"
                ).ToList();

                user = _unitOfWork.User.Get(u => u.Id == userIdInt);
            }
            else
            {
                var raw = HttpContext.Session.GetString("CartSession");

                if (!string.IsNullOrEmpty(raw))
                {
                    try
                    {
                        items = JsonSerializer.Deserialize<List<CartItem>>(raw) ?? new List<CartItem>();

                        foreach (var it in items)
                        {
                            if (it.ProductId.HasValue)
                            {
                                it.Product = _unitOfWork.Product.Get(p => p.ProductId == it.ProductId)
                                             ?? throw new Exception($"Product not found for ProductId={it.ProductId}");
                            }
                            else if (it.MealPlanDetailId.HasValue)
                            {
                                it.MealPlanDetail = _unitOfWork.MealPlan.Get(
                                    m => m.MealPlanDetailId == it.MealPlanDetailId,
                                    includeProperties: "MealPlanProductDetails.Product")
                                    ?? throw new Exception($"MealPlanDetail not found for MealPlanDetailId={it.MealPlanDetailId}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log deserialization or DB retrieval errors here as needed
                        Console.WriteLine($"LoadCartItems deserialization error: {ex.Message}");
                        items = new List<CartItem>();
                    }
                }
            }

            return items;
        }




        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            int Id = int.Parse(response.OrderId);
            if (response.VnPayResponseCode == "00")
            {
                _unitOfWork.Order.UpdateStatus(Id, SD.StatusApproved, SD.PaymentStatusApproved);
                var payment = new Payment
                {
                    OrderId = Id,
                    Method = response.PaymentMethod,
                    Status = "Completed",
                    PaymentDate = DateTime.Now,
                    Amount = response.Amount
                };
                _unitOfWork.Payment.Add(payment);
                _unitOfWork.Save();

                TempData["success"] = "Thanh toán thành công!";
            }
            else
            {
                TempData["error"] = "Thanh toán thất bại!";
            }
            return RedirectToAction("OrderConfirmation", "Cart", new { id = Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var order = _unitOfWork.Order.Get(u => u.OrderId == id, includeProperties: "User");
            if (order == null)
                return NotFound();

            if (order.PaymentStatus == SD.PaymentStatusApproved)
            {
                // Giảm số lượng tồn kho
                var orderDetails = _unitOfWork.OrderProductDetail.GetAll(u => u.OrderId == id, includeProperties: "Product");
                var orderComboDetails = _unitOfWork.OrderMealPlanDetail.GetAll(u => u.OrderId == id, includeProperties: "MealPlanDetail");

                foreach (var detail in orderDetails)
                {
                    var product = detail.Product;
                    if (product != null)
                    {
                        product.Quantity -= detail.Quantity;
                        if (product.Quantity < 0) product.Quantity = 0;
                        _unitOfWork.Product.Update(product);
                    }
                }

                // === Xử lý combo MealPlan ===
                var orderCombos = _unitOfWork.OrderMealPlanDetail.GetAll(
                    o => o.OrderId == id,
                    includeProperties: "MealPlanDetail.MealPlanProductDetails.Product");

                foreach (var combo in orderCombos)
                {
                    var details = combo.MealPlanDetail?.MealPlanProductDetails;
                    if (details != null)
                    {
                        foreach (var item in details)
                        {
                            var product = item.Product;
                            if (product != null)
                            {
                                product.Quantity -= item.Quantity * combo.Quantity;
                                if (product.Quantity < 0) product.Quantity = 0;
                                _unitOfWork.Product.Update(product);
                            }
                        }
                    }
                }

                _unitOfWork.Save();

                // === Xóa giỏ hàng ===
                if (order.UserId != null)
                {
                    var cartItems = _unitOfWork.CartItem.GetAll(u => u.UserId == order.UserId).ToList();
                    _unitOfWork.CartItem.RemoveRange(cartItems);
                }
                else
                {
                    HttpContext.Session.Remove("CartSession");
                }

                _unitOfWork.Save();

                // === Gửi email xác nhận ===
                var mailContent = new System.Text.StringBuilder();
                mailContent.AppendLine($"<h2>Đơn hàng - Xác nhận thanh toán</h2>");
                mailContent.AppendLine($"<p>Xin chào <strong>{order.FullName}</strong>,</p>");
                mailContent.AppendLine("<p>Chúng tôi rất cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Dưới đây là thông tin chi tiết đơn hàng:</p>");

                // Thông tin khách hàng
                mailContent.AppendLine("<h3>Thông tin khách hàng:</h3><ul>");
                mailContent.AppendLine($"<li>Họ và tên: {order.FullName}</li>");
                mailContent.AppendLine($"<li>Địa chỉ: {order.Address}, {order.City}, {order.Country}</li>");
                mailContent.AppendLine($"<li>Số điện thoại: {order.PhoneNumber}</li>");
                mailContent.AppendLine($"<li>Email: {order.Email}</li>");
                mailContent.AppendLine("</ul>");

                // Bảng chi tiết đơn hàng
                mailContent.AppendLine("<h3>Chi tiết đơn hàng:</h3>");
                mailContent.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");
                mailContent.AppendLine("<thead><tr>");
                mailContent.AppendLine("<th>Sản phẩm</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th>");
                mailContent.AppendLine("</tr></thead>");
                mailContent.AppendLine("<tbody>");

                decimal totalProductAmount = 0;
                decimal totalComboAmount = 0;

                // Sản phẩm lẻ
                foreach (var detail in orderDetails)
                {
                    var product = detail.Product;
                    if (product != null)
                    {
                        decimal subTotal = detail.UnitPrice * detail.Quantity;
                        totalProductAmount += subTotal;

                        mailContent.AppendLine("<tr>");
                        mailContent.AppendLine($"<td>{product.Name}</td>");
                        mailContent.AppendLine($"<td style='text-align:center;'>{detail.Quantity}</td>");
                        mailContent.AppendLine($"<td style='text-align:right;'>{detail.UnitPrice.ToString("N0")} VNĐ</td>");
                        mailContent.AppendLine($"<td style='text-align:right;'>{subTotal.ToString("N0")} VNĐ</td>");
                        mailContent.AppendLine("</tr>");
                    }
                }
                
                // Combo
                foreach (var combo in orderComboDetails)
                {
                    if (combo.MealPlanDetail != null)
                    {
                        decimal subTotal = combo.UnitPrice * combo.Quantity;
                        totalComboAmount += subTotal;
                        mailContent.AppendLine("<tr>");
                        mailContent.AppendLine($"<td>Combo: {combo.MealPlanDetail.PlanDescription}</td>");
                        mailContent.AppendLine($"<td style='text-align:center;'>{combo.Quantity}</td>");
                        mailContent.AppendLine($"<td style='text-align:right;'>{combo.UnitPrice.ToString("N0")} VNĐ</td>");
                        mailContent.AppendLine($"<td style='text-align:right;'>{subTotal.ToString("N0")} VNĐ</td>");
                        mailContent.AppendLine("</tr>");
                    }
                }

                // Chi tiết combo (chỉ hiển thị thông tin, không tính lại tiền)
                if (orderCombos.Any())
                {
                    mailContent.AppendLine("<h3>Chi tiết combo đã đặt:</h3>");
                    mailContent.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");
                    mailContent.AppendLine("<thead><tr><th>Sản phẩm trong combo</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead><tbody>");

                    foreach (var combo in orderCombos)
                    {
                        var meal = combo.MealPlanDetail;
                        if (meal != null && meal.MealPlanProductDetails != null)
                        {
                            foreach (var productDetail in meal.MealPlanProductDetails)
                            {
                                var product = productDetail.Product;
                                if (product != null)
                                {
                                    decimal subTotal = product.Price * productDetail.Quantity * combo.Quantity;
                                    
                                    mailContent.AppendLine("<tr>");
                                    mailContent.AppendLine($"<td>{product.Name}</td>");
                                    mailContent.AppendLine($"<td style='text-align:center;'>{productDetail.Quantity * combo.Quantity}</td>");
                                    mailContent.AppendLine($"<td style='text-align:right;'>{product.Price:N0} VNĐ</td>");
                                    mailContent.AppendLine($"<td style='text-align:right;'>{subTotal:N0} VNĐ</td>");
                                    mailContent.AppendLine("</tr>");
                                }
                            }
                        }
                    }

                    mailContent.AppendLine("</tbody></table>");
                }

                // Tổng tiền chi tiết
                decimal totalAmount = totalProductAmount + totalComboAmount;
                
                mailContent.AppendLine("<h3>Tổng tiền chi tiết:</h3>");
                mailContent.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%;'>");
                mailContent.AppendLine("<tbody>");
                mailContent.AppendLine("<tr>");
                mailContent.AppendLine($"<td><strong>Tổng tiền sản phẩm lẻ:</strong></td>");
                mailContent.AppendLine($"<td style='text-align:right;'><strong>{totalProductAmount:N0} VNĐ</strong></td>");
                mailContent.AppendLine("</tr>");
                mailContent.AppendLine("<tr>");
                mailContent.AppendLine($"<td><strong>Tổng tiền combo:</strong></td>");
                mailContent.AppendLine($"<td style='text-align:right;'><strong>{totalComboAmount:N0} VNĐ</strong></td>");
                mailContent.AppendLine("</tr>");
                mailContent.AppendLine("<tr style='background-color:#f8f9fa;'>");
                mailContent.AppendLine($"<td><strong>TỔNG TIỀN ĐƠN HÀNG:</strong></td>");
                mailContent.AppendLine($"<td style='text-align:right;'><strong style='color:#28a745; font-size:18px;'>{totalAmount:N0} VNĐ</strong></td>");
                mailContent.AppendLine("</tr>");
                mailContent.AppendLine("</tbody></table>");
                mailContent.AppendLine("<p>Chúng tôi sẽ liên hệ với bạn sớm nhất để xác nhận và giao hàng.</p>");
                mailContent.AppendLine("<p><em>Đội ngũ chăm sóc khách hàng</em></p>");

                // Gửi email
                var mailService = new SendMail(_configuration);
                mailService.Send(order.Email, $"Đơn hàng của bạn đã được xác nhận", mailContent.ToString());
            }

            return View(id);
        }


        // AddToCart method - Fixed to check stock correctly, including combos sharing ingredients
        [HttpPost]
        public IActionResult AddToCart(int? productId, int? mealPlanDetailId, int quantity = 1)
        {
            if (quantity < 1)
            {
                TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            if (!productId.HasValue && !mealPlanDetailId.HasValue)
            {
                TempData["ErrorMessage"] = "Không xác định được sản phẩm hoặc combo.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            string? userId = GetUserId();
            List<CartItem> userCart = GetCartItemsForUserOrSession(userId);

            int stockQty = 0;

            if (productId.HasValue)
            {
                var product = _unitOfWork.Product.Get(p => p.ProductId == productId.Value);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                stockQty = product.Quantity;

                // Tính số lượng nguyên liệu đã dùng trong combo
                int reservedQtyInCombos = userCart
                    .Where(ci => ci.MealPlanDetail?.MealPlanProductDetails != null)
                    .SelectMany(ci => ci.MealPlanDetail.MealPlanProductDetails
                        .Where(mp => mp.ProductId == productId.Value)
                        .Select(mp => mp.Quantity * ci.Quantity))
                    .Sum();

                stockQty -= reservedQtyInCombos;
            }
            else if (mealPlanDetailId.HasValue)
            {
                var mealPlan = _unitOfWork.MealPlan.Get(
                    m => m.MealPlanDetailId == mealPlanDetailId.Value,
                    includeProperties: "MealPlanProductDetails.Product");

                if (mealPlan == null)
                {
                    TempData["ErrorMessage"] = "Combo không tồn tại.";
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                // Tính tồn kho thực tế của combo dựa trên nguyên liệu còn lại
                int minAvailable = mealPlan.MealPlanProductDetails.Select(mp =>
                {
                    int realStock = mp.Product.Quantity;

                    int reserved = userCart
                        .Where(ci => ci.MealPlanDetail?.MealPlanProductDetails != null)
                        .SelectMany(ci => ci.MealPlanDetail.MealPlanProductDetails
                            .Where(d => d.ProductId == mp.ProductId)
                            .Select(d => d.Quantity * ci.Quantity))
                        .Sum();

                    return (realStock - reserved) / mp.Quantity;
                }).DefaultIfEmpty(0).Min();

                stockQty = minAvailable;
            }

            if (stockQty <= 0)
            {
                TempData["ErrorMessage"] = "Hiện tại sản phẩm/combo không còn đủ nguyên liệu do đã được sử dụng trong giỏ hàng.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Kiểm tra số lượng hiện có trong giỏ
            var existingItem = userCart.FirstOrDefault(c =>
                c.ProductId == productId &&
                c.MealPlanDetailId == mealPlanDetailId);

            int currentQty = existingItem?.Quantity ?? 0;

            if (currentQty + quantity > stockQty)
            {
                TempData["ErrorMessage"] = $"Bạn chỉ có thể thêm tối đa {Math.Max(stockQty - currentQty, 0)} sản phẩm/combo do giới hạn tồn kho. " +
                                           $"(Trong giỏ hiện có: {currentQty}, còn có thể thêm: {Math.Max(stockQty - currentQty, 0)}).";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Thêm mới hoặc cập nhật giỏ hàng
            if (!string.IsNullOrEmpty(userId))
            {
                int intUserId = int.Parse(userId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.AddedDate = DateTime.Now;
                    _unitOfWork.CartItem.Update(existingItem);
                }
                else
                {
                    var newItem = new CartItem
                    {
                        UserId = intUserId,
                        ProductId = productId,
                        MealPlanDetailId = mealPlanDetailId,
                        Quantity = quantity,
                        AddedDate = DateTime.Now
                    };
                    _unitOfWork.CartItem.Add(newItem);
                }

                _unitOfWork.Save();
            }
            else
            {
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.AddedDate = DateTime.Now;
                }
                else
                {
                    userCart.Add(new CartItem
                    {
                        CartItemId = new Random().Next(1, 999999),
                        ProductId = productId,
                        MealPlanDetailId = mealPlanDetailId,
                        Quantity = quantity,
                        AddedDate = DateTime.Now
                    });
                }

                HttpContext.Session.SetString("CartSession", JsonSerializer.Serialize(userCart));
            }

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng.";
            return Redirect(Request.Headers["Referer"].ToString());
        }


        private List<CartItem> GetCartItemsForUserOrSession(string? userId)
        {
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
            {
                return _unitOfWork.CartItem.GetAll(
                    c => c.UserId == userIdInt,
                    includeProperties: "MealPlanDetail.MealPlanProductDetails.Product")
                    .ToList();
            }
            else
            {
                var sessionCart = HttpContext.Session.GetString("CartSession");
                var cartItems = string.IsNullOrEmpty(sessionCart)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(sessionCart);

                foreach (var it in cartItems)
                {
                    if (it.MealPlanDetailId.HasValue)
                        it.MealPlanDetail = _unitOfWork.MealPlan.Get(
                            m => m.MealPlanDetailId == it.MealPlanDetailId,
                            includeProperties: "MealPlanProductDetails.Product");
                }

                return cartItems;
            }
        }


    }
}