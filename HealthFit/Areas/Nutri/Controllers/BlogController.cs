using System.Security.Claims;
using HealthFit.DataAccess.Data;
using HealthFit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace HealthFit.Areas.Nutri.Controllers
{
    [Area("Nutri")]
    public class BlogController : Controller
    {
        private readonly HealthyShopContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogController(HealthyShopContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // Hiển thị danh sách blog với tìm kiếm và phân trang
        public IActionResult Index(string searchType, string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["SearchType"] = searchType;

            var blogs = _db.Blogs.Include(b => b.User).AsQueryable();

            // Lọc theo tiêu đề
            if (searchType == "Title" && !string.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(s => s.Title.Contains(searchString));
            }

            // Lọc theo BMI
            if (searchType == "Bmirange" && !string.IsNullOrEmpty(searchString))
            {
                if (double.TryParse(searchString, out double bmiValue))
                {
                    blogs = blogs.Where(b => b.Bmirange.HasValue && (
                        (bmiValue < 18.5 && b.Bmirange < 18.5) ||
                        (bmiValue >= 18.5 && bmiValue <= 22.9 && b.Bmirange >= 18.5 && b.Bmirange <= 22.9) ||
                        (bmiValue >= 23 && bmiValue <= 24.9 && b.Bmirange >= 23 && b.Bmirange <= 24.9) ||
                        (bmiValue >= 25 && bmiValue <= 29.9 && b.Bmirange >= 25 && b.Bmirange <= 29.9) ||
                        (bmiValue >= 30 && b.Bmirange >= 30)
                    ));
                }
                else
                {
                    blogs = Enumerable.Empty<Blog>().AsQueryable();
                    TempData["error"] = "Vui lòng nhập BMI là một số, ví dụ: 20.5";
                }
            }


            int pageNumber = page ?? 1;
            int pageSize = 5;
            var pagedBlogs = blogs.OrderBy(b => b.BlogId).ToPagedList(pageNumber, pageSize);

            if (!pagedBlogs.Any() && !string.IsNullOrEmpty(searchString))
            {
                TempData["error"] = "Không tìm thấy blog phù hợp với tiêu chí tìm kiếm.";
            }

            return View(pagedBlogs);
        }



        // Xem chi tiết blog
        public IActionResult ReadMore(int? BlogId)
        {
            if (BlogId == null || BlogId == 0)
                return NotFound();

            var blog = _db.Blogs.Include(b => b.User).FirstOrDefault(b => b.BlogId == BlogId);
            if (blog == null)
                return NotFound();

            return View(blog);
        }

        // GET: CreateBlog
        [HttpGet]
        public IActionResult CreateBlog()
        {
            return View();
        }

        // POST: CreateBlog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBlog(Blog obj, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userId, out int parsedUserId))
                    obj.UserId = parsedUserId;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    string uploadPath = Path.Combine(wwwRootPath, "images/blogs");

                    Directory.CreateDirectory(uploadPath);
                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        ImageFile.CopyTo(fileStream);
                    }

                    obj.ImageUrl = "/images/blogs/" + fileName;
                }

                obj.CreatedDate = DateTime.Today;
                obj.LastUpdated = DateTime.Now;

                _db.Blogs.Add(obj);
                _db.SaveChanges();

                TempData["success"] = "Blog đã được tạo thành công!";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        // GET: Edit
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var blogFromDb = _db.Blogs.FirstOrDefault(b => b.BlogId == id);
            if (blogFromDb == null)
                return NotFound();

            return View(blogFromDb);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Blog obj, IFormFile ImageFile)
        {
            // Lấy blog gốc để giữ lại các field cũ
            var blogFromDb = _db.Blogs.AsNoTracking().FirstOrDefault(b => b.BlogId == obj.BlogId);
            if (blogFromDb == null)
                return NotFound();

            // 🔥 Rất quan trọng: gán lại các trường cũ trước khi validate
            obj.CreatedDate = blogFromDb.CreatedDate;
            obj.UserId = blogFromDb.UserId;

            if (ImageFile == null || ImageFile.Length == 0)
            {
                obj.ImageUrl = blogFromDb.ImageUrl; // ✅ giữ ảnh cũ nếu không upload mới
            }

            if (ModelState.IsValid)
            {
                // Nếu có ảnh mới
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Xoá ảnh cũ
                    if (!string.IsNullOrEmpty(blogFromDb.ImageUrl))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, blogFromDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Upload ảnh mới
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    string uploadPath = Path.Combine(wwwRootPath, "images/blogs");
                    Directory.CreateDirectory(uploadPath);

                    using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    obj.ImageUrl = "/images/blogs/" + fileName;
                }

                obj.LastUpdated = DateTime.Now;
                _db.Blogs.Update(obj);
                _db.SaveChanges();

                TempData["success"] = "Blog đã được cập nhật thành công!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View(obj);
        }



        // GET: Delete
        public IActionResult Delete(int? BlogId)
        {
            if (BlogId == null || BlogId == 0)
                return NotFound();

            var blogFromDb = _db.Blogs.Find(BlogId);
            if (blogFromDb == null)
                return NotFound();

            return View(blogFromDb);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? BlogId)
        {
            var obj = _db.Blogs.Find(BlogId);
            if (obj == null)
                return NotFound();

            _db.Blogs.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Blog deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
