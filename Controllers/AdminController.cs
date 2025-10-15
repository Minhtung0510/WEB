using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoBikeStore.Models;
using System.Linq;

namespace MotoBikeStore.Controllers
{
    public class AdminController : Controller
    {
        private readonly MotoBikeContext _db;
        const string USER_KEY = "CURRENT_USER";
        
        public AdminController(MotoBikeContext db) => _db = db;
        
        // Kiểm tra quyền admin
        private bool IsAdmin()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<SessionUser>(USER_KEY);
            return currentUser != null && currentUser.Role == "Admin";
        }
        
        // Dashboard
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            
            // Thống kê tổng quan
            ViewBag.TotalOrders = _db.Orders.Count();
            ViewBag.TotalRevenue = _db.Orders.Sum(o => (decimal?)o.Total) ?? 0;
            ViewBag.TotalProducts = _db.Products.Count();
            ViewBag.TotalCustomers = _db.Users.Count(u => u.Role == "Customer");
            
            // Doanh thu tháng này
            ViewBag.MonthlyRevenue = _db.Orders
                .Where(o => o.OrderDate >= thisMonth)
                .Sum(o => (decimal?)o.Total) ?? 0;
            
            // Đơn hàng chưa xử lý
            ViewBag.PendingOrders = _db.Orders.Count(o => o.Status == "Pending");
            
            // Top sản phẩm bán chạy
            var topProducts = _db.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.ProductId)
                .Select(g => new {
                    Product = g.First().Product,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();
            ViewBag.TopProducts = topProducts;
            
            // Đơn hàng gần đây
            var recentOrders = _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();
            ViewBag.RecentOrders = recentOrders;
            
            return View();
        }
        
        // Quản lý đơn hàng
        public IActionResult Orders(string? status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);
            
            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
            
            ViewBag.SelectedStatus = status;
            return View(orders);
        }
        
        // Chi tiết đơn hàng
        public IActionResult OrderDetail(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var order = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.Id == id);
            
            if (order == null) return NotFound();
            return View(order);
        }
        
        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string status, string? trackingNumber)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var order = _db.Orders.Find(id);
            if (order == null) return NotFound();
            
            order.Status = status;
            
            if (status == "Shipping" && !string.IsNullOrEmpty(trackingNumber))
            {
                order.TrackingNumber = trackingNumber;
                order.ShippedDate = DateTime.UtcNow;
            }
            else if (status == "Delivered")
            {
                order.DeliveredDate = DateTime.UtcNow;
            }
            
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái đơn hàng!";
            return RedirectToAction("OrderDetail", new { id });
        }
        
        // Báo cáo doanh thu
        public IActionResult Reports(int? year, int? month)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            year ??= DateTime.UtcNow.Year;
            month ??= DateTime.UtcNow.Month;
            
            // Doanh thu theo tháng trong năm
            var monthlyRevenue = _db.Orders
                .Where(o => o.OrderDate.Year == year && o.Status != "Cancelled")
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();
            
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.Year = year;
            ViewBag.Month = month;
            
            // Doanh thu theo ngày trong tháng
            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1);
            
            var dailyRevenue = _db.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.Status != "Cancelled")
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();
            
            ViewBag.DailyRevenue = dailyRevenue;
            
            // Top khách hàng
            var topCustomers = _db.Orders
                .Where(o => o.UserId != null && o.Status != "Cancelled")
                .GroupBy(o => o.UserId)
                .Select(g => new {
                    UserId = g.Key,
                    TotalSpent = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToList();
            
            var customerIds = topCustomers.Select(c => c.UserId).ToList();
            var customers = _db.Users.Where(u => customerIds.Contains(u.Id)).ToList();
            
            ViewBag.TopCustomers = topCustomers.Select(tc => new {
                Customer = customers.First(c => c.Id == tc.UserId),
                tc.TotalSpent,
                tc.OrderCount
            }).ToList();
            
            return View();
        }
    }
}