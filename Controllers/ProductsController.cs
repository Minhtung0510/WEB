using Microsoft.AspNetCore.Mvc;
using MotoBikeStore.Models;
using System.Linq;
namespace MotoBikeStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MotoBikeContext _db;
        public ProductsController(MotoBikeContext db) => _db = db;
        public IActionResult Index() => View(_db.Products.ToList());
        public IActionResult Create() => View();
        [HttpPost] public IActionResult Create(Product p){ if(!ModelState.IsValid) return View(p); _db.Products.Add(p); _db.SaveChanges(); return RedirectToAction(nameof(Index)); }
        public IActionResult Edit(int id){ var p=_db.Products.Find(id); if(p==null) return NotFound(); return View(p); }
        [HttpPost] public IActionResult Edit(Product p){ if(!ModelState.IsValid) return View(p); _db.Update(p); _db.SaveChanges(); return RedirectToAction(nameof(Index)); }
        public IActionResult Delete(int id){ var p=_db.Products.Find(id); if(p==null) return NotFound(); return View(p); }
        [HttpPost,ActionName("Delete")] public IActionResult DeleteConfirmed(int id){ var p=_db.Products.Find(id); if(p!=null){ _db.Products.Remove(p); _db.SaveChanges(); } return RedirectToAction(nameof(Index)); }
    }
}
