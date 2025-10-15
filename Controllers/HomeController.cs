using Microsoft.AspNetCore.Mvc;
using MotoBikeStore.Models;
using System.Linq;
namespace MotoBikeStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly MotoBikeContext _db;
        public HomeController(MotoBikeContext db) => _db = db;
        public IActionResult Index(string? brand, string? q)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(brand)) query = query.Where(p => p.Brand == brand);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q));
            return View(query.OrderBy(p=>p.Brand).ToList());
        }
    }
}
