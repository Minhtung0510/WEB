using Microsoft.AspNetCore.Mvc;
using MotoBikeStore.Models;
using System.Linq;
using System.Collections.Generic;

namespace MotoBikeStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly MotoBikeContext _db;
        const string CART_KEY = "CART_ITEMS";
        public OrdersController(MotoBikeContext db) => _db = db;

        public IActionResult Checkout() => View(new Order());

        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<int>>(CART_KEY) ?? new List<int>();
            if (!items.Any()){ ModelState.AddModelError("", "Giỏ hàng trống."); return View(order); }
            if (!ModelState.IsValid) return View(order);

            foreach (var id in items)
            {
                var p = _db.Products.Find(id);
                if (p == null) continue;
                order.Details.Add(new OrderDetail { ProductId=p.Id, Quantity=1, UnitPrice=p.Price });
            }
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Confirmed";
            _db.Orders.Add(order);
            _db.SaveChanges();
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Success", new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            var order = _db.Orders.Find(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
