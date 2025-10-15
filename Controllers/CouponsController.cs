using Microsoft.AspNetCore.Mvc;
using MotoBikeStore.Models;
using System.Linq;

namespace MotoBikeStore.Controllers
{
    public class CouponsController : Controller
    {
        private readonly MotoBikeContext _db;
        const string USER_KEY = "CURRENT_USER";
        
        public CouponsController(MotoBikeContext db) => _db = db;
        
        private bool IsAdmin()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<dynamic>(USER_KEY);
            return currentUser != null && currentUser.Role == "Admin";
        }
        
        // Danh sách mã giảm giá
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var coupons = _db.Coupons.OrderByDescending(c => c.StartDate).ToList();
            return View(coupons);
        }
        
        // Tạo mã giảm giá
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }
        
        [HttpPost]
        public IActionResult Create(Coupon coupon)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            if (_db.Coupons.Any(c => c.Code.ToLower() == coupon.Code.ToLower()))
            {
                ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại");
                return View(coupon);
            }
            
            if (!ModelState.IsValid) return View(coupon);
            
            coupon.UsedCount = 0;
            _db.Coupons.Add(coupon);
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Tạo mã giảm giá thành công!";
            return RedirectToAction("Index");
        }
        
        // Sửa mã giảm giá
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var coupon = _db.Coupons.Find(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }
        
        [HttpPost]
        public IActionResult Edit(Coupon coupon)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            if (!ModelState.IsValid) return View(coupon);
            
            _db.Update(coupon);
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công!";
            return RedirectToAction("Index");
        }
        
        // Xóa mã giảm giá
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var coupon = _db.Coupons.Find(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }
        
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var coupon = _db.Coupons.Find(id);
            if (coupon != null)
            {
                _db.Coupons.Remove(coupon);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa mã giảm giá thành công!";
            }
            
            return RedirectToAction("Index");
        }
        
        // Toggle Active/Inactive
        public IActionResult ToggleStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var coupon = _db.Coupons.Find(id);
            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                _db.SaveChanges();
                TempData["SuccessMessage"] = coupon.IsActive 
                    ? "Đã kích hoạt mã giảm giá!" 
                    : "Đã vô hiệu hóa mã giảm giá!";
            }
            
            return RedirectToAction("Index");
        }
        
        // API: Kiểm tra mã giảm giá (cho khách hàng)
        [HttpPost]
        public JsonResult Validate(string code, decimal orderAmount)
        {
            var coupon = _db.Coupons.FirstOrDefault(c => 
                c.Code.ToLower() == code.ToLower() && 
                c.IsActive && 
                c.StartDate <= DateTime.UtcNow && 
                c.EndDate >= DateTime.UtcNow &&
                (c.UsageLimit == 0 || c.UsedCount < c.UsageLimit)
            );
            
            if (coupon == null)
                return Json(new { valid = false, message = "Mã giảm giá không hợp lệ" });
            
            if (orderAmount < coupon.MinOrderAmount)
                return Json(new { 
                    valid = false, 
                    message = $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0}₫ để áp dụng mã này" 
                });
            
            decimal discount = 0;
            if (coupon.DiscountPercent > 0)
            {
                discount = orderAmount * coupon.DiscountPercent / 100;
                if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                    discount = coupon.MaxDiscountAmount.Value;
            }
            else if (coupon.DiscountAmount.HasValue)
            {
                discount = coupon.DiscountAmount.Value;
            }
            
            return Json(new { 
                valid = true, 
                discount = discount,
                message = $"Giảm {discount:N0}₫",
                description = coupon.Description
            });
        }
    }
}