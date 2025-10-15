using Microsoft.AspNetCore.Mvc;
using MotoBikeStore.Models;
using System.Linq;
using System.Collections.Generic;

namespace MotoBikeStore.Controllers
{
    public class CartController : Controller
    {
        private readonly MotoBikeContext _db;
        public CartController(MotoBikeContext db) => _db = db;
        const string CART_KEY = "CART_ITEMS";

        public IActionResult Index()
        {
            var items = HttpContext.Session.GetObjectFromJson<List<int>>(CART_KEY) ?? new List<int>();
            var products = _db.Products.Where(p => items.Contains(p.Id)).ToList();
            return View(products);
        }
        public IActionResult Add(int id)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<int>>(CART_KEY) ?? new List<int>();
            if (!items.Contains(id)) items.Add(id);
            HttpContext.Session.SetObjectAsJson(CART_KEY, items);
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int id)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<int>>(CART_KEY) ?? new List<int>();
            items.Remove(id);
            HttpContext.Session.SetObjectAsJson(CART_KEY, items);
            return RedirectToAction("Index");
        }
    }
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value) =>
            session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }
    }
}
