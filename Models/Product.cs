namespace MotoBikeStore.Models
{
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
        public string? Badge { get; set; } // "new" / "hot"
    }
}
