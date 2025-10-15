using Microsoft.EntityFrameworkCore;
using MotoBikeStore.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MotoBikeContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("MotoBikeDB")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.Cookie.Name = ".MotoBikeStore.Session"; o.IdleTimeout = TimeSpan.FromHours(2); o.Cookie.HttpOnly = true; });
var app = builder.Build();
if (!app.Environment.IsDevelopment()){ app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope()){
    var ctx = scope.ServiceProvider.GetRequiredService<MotoBikeContext>();
    ctx.Database.EnsureCreated();
    DbSeeder.Seed(ctx);
}
app.Run();
