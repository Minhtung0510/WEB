using Microsoft.EntityFrameworkCore;
namespace MotoBikeStore.Models
{
    public class MotoBikeContext : DbContext
    {
        public MotoBikeContext(DbContextOptions<MotoBikeContext> options) : base(options) {}
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    }
    public static class DbSeeder
    {
        public static void Seed(MotoBikeContext db)
        {
            if (db.Products.Any()) return;
            db.Products.AddRange(
  new Product { Name="Honda Air Blade 160", Brand="Honda", Engine="160cc", Fuel="5.5L",
    Rating=4.8m, Price=48990000, OldPrice=57500000, DiscountPercent=15, ImageUrl="/images/airblade160.jpg" },
  new Product { Name="Yamaha Exciter 155 VVA", Brand="Yamaha", Engine="155cc", Fuel="5.0L",
    Rating=4.9m, Price=52490000, Badge="new", ImageUrl="/images/exciter155.jpg" },
  new Product { Name="Honda Vision 2024", Brand="Honda", Engine="110cc", Fuel="5.2L",
    Rating=4.7m, Price=32990000, ImageUrl="/images/vision2024.jpg" },
  new Product { Name="Yamaha Janus Premium", Brand="Yamaha", Engine="125cc", Fuel="5.5L",
    Rating=4.6m, Price=33500000, Badge="hot", ImageUrl="/images/janus.jpg" }
);
            db.SaveChanges();
        }
    }
}
