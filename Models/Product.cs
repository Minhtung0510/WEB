using System.ComponentModel.DataAnnotations;

namespace MotoBikeStore.Models
{
    // Product - Cập nhật
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Engine { get; set; } = "";
        public string Fuel { get; set; } = "";
        public decimal Rating { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImageUrl { get; set; } = "";
        public int? DiscountPercent { get; set; }
        public string? Badge { get; set; }
        
        // MỚI: Thêm Category
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        
        // MỚI: Thông số kỹ thuật chi tiết (cho so sánh)
        public string? Description { get; set; }
        public int? Stock { get; set; } = 100; // Tồn kho
        public string? Color { get; set; }
        public string? Warranty { get; set; } // Bảo hành
    }
    
    // Order - Cập nhật
    public class Order
    {
        public int Id { get; set; }
        
        // MỚI: Liên kết với User
        public int? UserId { get; set; }
        public User? User { get; set; }
        
        [Required] public string CustomerName { get; set; } = "";
        [Required] public string Phone { get; set; } = "";
        [Required] public string Address { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        // MỚI: Trạng thái chi tiết
        public string Status { get; set; } = "Pending"; 
        // Pending -> Confirmed -> Processing -> Shipping -> Delivered -> Cancelled
        
        // MỚI: Tracking
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        
        // MỚI: Coupon
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public decimal Total { get; set; }
        
        public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }
    
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Product? Product { get; set; }
        public Order? Order { get; set; }
    }
}