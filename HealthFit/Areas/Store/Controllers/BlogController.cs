using System.Security.Claims;
using HealthFit.DataAccess.Data;
using HealthFit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace HealthFit.Areas.Store.Controllers
{
    [Area("Store")]
    public class BlogController : Controller
    {
        private readonly HealthyShopContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogController(HealthyShopContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string searchString, int? page)
        {
            var blogs = _db.Blogs.Include(b => b.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(b => b.Title.Contains(searchString));
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            return View(blogs.OrderByDescending(b => b.CreatedDate).ToPagedList(pageNumber, pageSize));
        }

        public IActionResult ReadMore(int? id)
        {
            if (id == null)
                return NotFound();

            var blog = _db.Blogs.Include(b => b.User).FirstOrDefault(b => b.BlogId == id);
            if (blog == null)
                return NotFound();

            return View(blog);
        }
    }
}
