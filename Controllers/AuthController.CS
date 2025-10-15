using Microsoft.AspNetCore.Mvc;
using MotoBikeStore.Models;
using System.Linq;

namespace MotoBikeStore.Controllers
{
    public class AuthController : Controller
    {
        private readonly MotoBikeContext _db;
        const string USER_KEY = "CURRENT_USER";
        
        public AuthController(MotoBikeContext db) => _db = db;
        
        // GET: Auth/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        // POST: Auth/Login
        [HttpPost]
        public IActionResult Login(string email, string password, string? returnUrl = null)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email && u.IsActive);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View();
            }
            
            // Lưu thông tin user vào session
            HttpContext.Session.SetObjectAsJson(USER_KEY, new { 
                user.Id, user.Email, user.FullName, user.Role 
            });
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
                
            return user.Role == "Admin" 
                ? RedirectToAction("Index", "Admin") 
                : RedirectToAction("Index", "Home");
        }
        
        // GET: Auth/Register
        public IActionResult Register() => View();
        
        // POST: Auth/Register
        [HttpPost]
        public IActionResult Register(User user, string password, string confirmPassword)
        {
            if (_db.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(user);
            }
            
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                return View(user);
            }
            
            if (password.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự");
                return View(user);
            }
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Role = "Customer";
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;
            
            _db.Users.Add(user);
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
        
        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(USER_KEY);
            return RedirectToAction("Index", "Home");
        }
        
        // Profile
        public IActionResult Profile()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<dynamic>(USER_KEY);
            if (currentUser == null) return RedirectToAction("Login");
            
            var user = _db.Users.Find((int)currentUser.Id);
            if (user == null) return RedirectToAction("Logout");
            
            return View(user);
        }
        
        // POST: Update Profile
        [HttpPost]
        public IActionResult Profile(User updatedUser)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<dynamic>(USER_KEY);
            if (currentUser == null) return RedirectToAction("Login");
            
            var user = _db.Users.Find((int)currentUser.Id);
            if (user == null) return RedirectToAction("Logout");
            
            user.FullName = updatedUser.FullName;
            user.Phone = updatedUser.Phone;
            user.Address = updatedUser.Address;
            
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }
    }
}