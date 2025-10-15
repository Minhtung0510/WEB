using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoBikeStore.Models;
using System.Linq;
using System.Collections.Generic;
using MotoBikeStore.Services;

namespace MotoBikeStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly MotoBikeContext _db;
        const string CART_KEY = "CART_ITEMS";
        const string USER_KEY = "CURRENT_USER";
        
        public OrdersController(MotoBikeContext db) => _db = db;

        public IActionResult Checkout() 
        {
            var items = HttpContext.Session.GetObjectFromJson<List<int>>(CART_KEY) ?? new List<int>();
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }
            
            var products = _db.Products.Where(p => items.Contains(p.Id)).ToList();
            ViewBag.Products = products;
            ViewBag.Subtotal = products.Sum(p => p.Price);
            
            // Tự động điền thông tin nếu đã đăng nhập
            var currentUser = HttpContext.Session.GetObjectFromJson<SessionUser>(USER_KEY);
            if (currentUser != null)
            {
                var user = _db.Users.Find(currentUser.Id);
                return View(new Order { 
                    CustomerName = user?.FullName ?? "", 
                    Phone = user?.Phone ?? "",
                    Address = user?.Address ?? "",
                    Email = user?.Email ?? ""
                });
            }
            
            return View(new Order());
        }

        [HttpPost]
        public IActionResult Checkout(Order order, string? couponCode)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<int>>(CART_KEY) ?? new List<int>();
            if (!items.Any())
            { 
                ModelState.AddModelError("", "Giỏ hàng trống."); 
                return View(order); 
            }
            
            if (!ModelState.IsValid) return View(order);

            var products = _db.Products.Where(p => items.Contains(p.Id)).ToList();
            decimal subtotal = 0;
            
            foreach (var id in items)
            {
                var p = products.FirstOrDefault(x => x.Id == id);
                if (p == null) continue;
                order.Details.Add(new OrderDetail { 
                    ProductId = p.Id, 
                    Quantity = 1, 
                    UnitPrice = p.Price 
                });
                subtotal += p.Price;
            }
            
            order.Subtotal = subtotal;
            order.ShippingFee = subtotal >= 5000000 ? 0 : 150000; // Miễn phí ship > 5 triệu
            order.DiscountAmount = 0;
            
            // Áp dụng coupon
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = _db.Coupons.FirstOrDefault(c => 
                    c.Code.ToLower() == couponCode.ToLower() && 
                    c.IsActive && 
                    c.StartDate <= DateTime.UtcNow && 
                    c.EndDate >= DateTime.UtcNow &&
                    (c.UsageLimit == 0 || c.UsedCount < c.UsageLimit)
                );
                
                if (coupon != null && subtotal >= coupon.MinOrderAmount)
                {
                    if (coupon.DiscountPercent > 0)
                    {
                        order.DiscountAmount = subtotal * coupon.DiscountPercent / 100;
                        if (coupon.MaxDiscountAmount.HasValue && order.DiscountAmount > coupon.MaxDiscountAmount.Value)
                            order.DiscountAmount = coupon.MaxDiscountAmount.Value;
                    }
                    else if (coupon.DiscountAmount.HasValue)
                    {
                        order.DiscountAmount = coupon.DiscountAmount.Value;
                    }
                    
                    order.CouponId = coupon.Id;
                    coupon.UsedCount++;
                }
                else
                {
                    TempData["CouponError"] = "Mã giảm giá không hợp lệ hoặc không đủ điều kiện";
                }
            }
            
            order.Total = order.Subtotal + order.ShippingFee - order.DiscountAmount;
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending";
            
            // Liên kết với user nếu đã đăng nhập
            var currentUser = HttpContext.Session.GetObjectFromJson<SessionUser>(USER_KEY);
            if (currentUser != null) order.UserId = currentUser.Id;
            
            _db.Orders.Add(order);
            _db.SaveChanges();
            
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Success", new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            var order = _db.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.Id == id);
                
            if (order == null) return NotFound();
            return View(order);
        }
        
        // Theo dõi đơn hàng
        public IActionResult Track(int? id)
        {
            if (id == null) return View((Order?)null);
            
            var order = _db.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.Id == id);
                
            return View(order);
        }
        
        // Danh sách đơn hàng của user
        public IActionResult MyOrders()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<SessionUser>(USER_KEY);
            if (currentUser == null) return RedirectToAction("Login", "Auth");

            var orders = InMemoryDataStore.Orders
                .Where(o => o.UserId == currentUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
                
            return View(orders);
        }
    }
}