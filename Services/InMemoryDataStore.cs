using MotoBikeStore.Models;
using MotoBikeStore.Services;

namespace MotoBikeStore.Services
{
    /// <summary>
    /// In-Memory Data Store - Thay thế Database
    /// Lưu trữ dữ liệu trong memory (RAM)
    /// </summary>
    public static class InMemoryDataStore
    {
        // Counters cho auto-increment IDs
        private static int _productIdCounter = 1;
        private static int _orderIdCounter = 1;
        private static int _userIdCounter = 1;
        private static int _categoryIdCounter = 1;
        private static int _couponIdCounter = 1;
        private static int _orderDetailIdCounter = 1;

        // Data storage - Sử dụng List<T> làm "bảng"
        public static List<Product> Products { get; set; } = new();
        public static List<Order> Orders { get; set; } = new();
        public static List<OrderDetail> OrderDetails { get; set; } = new();
        public static List<User> Users { get; set; } = new();
        public static List<Category> Categories { get; set; } = new();
        public static List<Coupon> Coupons { get; set; } = new();

        /// <summary>
        /// Khởi tạo dữ liệu mẫu
        /// </summary>
        public static void Initialize()
        {
            if (Products.Any()) return; // Đã khởi tạo rồi

            // 1. SEED CATEGORIES
            Categories = new List<Category>
            {
                new Category { Id = _categoryIdCounter++, Name = "Xe Tay Ga", Description = "Xe số tự động, tiện lợi đô thị" },
                new Category { Id = _categoryIdCounter++, Name = "Xe Số", Description = "Xe số côn, tiết kiệm nhiên liệu" },
                new Category { Id = _categoryIdCounter++, Name = "Xe Phân Khối Lớn", Description = "Xe PKL, mạnh mẽ" },
                new Category { Id = _categoryIdCounter++, Name = "Phụ Tùng", Description = "Phụ tùng chính hãng" }
            };

            // 2. SEED PRODUCTS
            Products = new List<Product>
            {
                new Product
                {
                    Id = _productIdCounter++,
                    Name = "Honda Air Blade 160",
                    Brand = "Honda",
                    Engine = "160cc",
                    Fuel = "5.5L",
                    Rating = 4.8m,
                    Price = 48990000,
                    OldPrice = 57500000,
                    DiscountPercent = 15,
                    ImageUrl = "/images/airblade160.jpg",
                    CategoryId = 1,
                    Description = "Thiết kế thể thao, động cơ eSP+",
                    Stock = 50,
                    Color = "Đỏ, Đen, Xám",
                    Warranty = "3 năm"
                },
                new Product
                {
                    Id = _productIdCounter++,
                    Name = "Yamaha Exciter 155 VVA",
                    Brand = "Yamaha",
                    Engine = "155cc",
                    Fuel = "5.0L",
                    Rating = 4.9m,
                    Price = 52490000,
                    Badge = "new",
                    ImageUrl = "/images/exciter155.jpg",
                    CategoryId = 2,
                    Description = "Công nghệ VVA, phanh ABS",
                    Stock = 30,
                    Color = "Xanh GP, Đen, Đỏ",
                    Warranty = "3 năm"
                },
                new Product
                {
                    Id = _productIdCounter++,
                    Name = "Honda Vision 2024",
                    Brand = "Honda",
                    Engine = "110cc",
                    Fuel = "5.2L",
                    Rating = 4.7m,
                    Price = 32990000,
                    ImageUrl = "/images/vision2024.jpg",
                    CategoryId = 1,
                    Description = "Sang trọng, tiết kiệm",
                    Stock = 80,
                    Color = "Bạc, Đen, Nâu",
                    Warranty = "3 năm"
                },
                new Product
                {
                    Id = _productIdCounter++,
                    Name = "Yamaha Janus Premium",
                    Brand = "Yamaha",
                    Engine = "125cc",
                    Fuel = "5.5L",
                    Rating = 4.6m,
                    Price = 33500000,
                    Badge = "hot",
                    ImageUrl = "/images/janus.jpg",
                    CategoryId = 1,
                    Description = "Thiết kế retro độc đáo",
                    Stock = 45,
                    Color = "Xanh Mint, Vàng, Hồng",
                    Warranty = "3 năm"
                }
            };

            // 3. SEED COUPONS
            Coupons = new List<Coupon>
            {
                new Coupon
                {
                    Id = _couponIdCounter++,
                    Code = "WELCOME10",
                    Description = "Giảm 10% cho đơn đầu tiên",
                    DiscountPercent = 10,
                    MinOrderAmount = 5000000,
                    MaxDiscountAmount = 2000000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    UsageLimit = 100,
                    IsActive = true
                },
                new Coupon
                {
                    Id = _couponIdCounter++,
                    Code = "SUMMER2024",
                    Description = "Khuyến mãi hè - Giảm 1 triệu",
                    DiscountAmount = 1000000,
                    MinOrderAmount = 30000000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    UsageLimit = 50,
                    IsActive = true
                },
                new Coupon
                {
                    Id = _couponIdCounter++,
                    Code = "VIP15",
                    Description = "Giảm 15% cho khách VIP",
                    DiscountPercent = 15,
                    MinOrderAmount = 10000000,
                    MaxDiscountAmount = 5000000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    UsageLimit = 0,
                    IsActive = true
                }
            };

            // 4. SEED ADMIN USER
            Users = new List<User>
            {
                new User
                {
                    Id = _userIdCounter++,
                    Email = "admin@motobike.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FullName = "Administrator",
                    Phone = "0901234567",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Orders và OrderDetails sẽ được thêm khi người dùng đặt hàng
        }

        // ============ HELPER METHODS ============

        /// <summary>
        /// Lấy ID mới cho Product
        /// </summary>
        public static int GetNextProductId() => _productIdCounter++;

        /// <summary>
        /// Lấy ID mới cho Order
        /// </summary>
        public static int GetNextOrderId() => _orderIdCounter++;

        /// <summary>
        /// Lấy ID mới cho User
        /// </summary>
        public static int GetNextUserId() => _userIdCounter++;

        /// <summary>
        /// Lấy ID mới cho Category
        /// </summary>
        public static int GetNextCategoryId() => _categoryIdCounter++;

        /// <summary>
        /// Lấy ID mới cho Coupon
        /// </summary>
        public static int GetNextCouponId() => _couponIdCounter++;

        /// <summary>
        /// Lấy ID mới cho OrderDetail
        /// </summary>
        public static int GetNextOrderDetailId() => _orderDetailIdCounter++;

        /// <summary>
        /// Reset tất cả dữ liệu (dùng cho testing)
        /// </summary>
        public static void Reset()
        {
            Products.Clear();
            Orders.Clear();
            OrderDetails.Clear();
            Users.Clear();
            Categories.Clear();
            Coupons.Clear();

            _productIdCounter = 1;
            _orderIdCounter = 1;
            _userIdCounter = 1;
            _categoryIdCounter = 1;
            _couponIdCounter = 1;
            _orderDetailIdCounter = 1;

            Initialize();
        }

        /// <summary>
        /// Populate navigation properties (giống như Include trong EF)
        /// </summary>
        public static void PopulateNavigationProperties()
        {
            // Link Products với Categories
            foreach (var product in Products)
            {
                product.Category = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
            }

            // Link Orders với Users
            foreach (var order in Orders)
            {
                order.User = Users.FirstOrDefault(u => u.Id == order.UserId);
                order.Coupon = Coupons.FirstOrDefault(c => c.Id == order.CouponId);
                
                // Link OrderDetails
                order.Details = OrderDetails.Where(od => od.OrderId == order.Id).ToList();
                
                foreach (var detail in order.Details)
                {
                    detail.Product = Products.FirstOrDefault(p => p.Id == detail.ProductId);
                }
            }
        }
    }
}