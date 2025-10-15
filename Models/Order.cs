using System.ComponentModel.DataAnnotations;
namespace MotoBikeStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required] public string CustomerName { get; set; } = "";
        [Required] public string Phone { get; set; } = "";
        [Required] public string Address { get; set; } = "";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
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
