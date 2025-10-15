using MotoBikeStore.Models;
using MotoBikeStore.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============ SERVICES ============
builder.Services.AddControllersWithViews();

// Session - BẮT BUỘC
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => {
    o.Cookie.Name = ".MotoBikeStore.Session";
    o.IdleTimeout = TimeSpan.FromHours(4);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// ❌ XÓA DÒNG NÀY NẾU CÓ:
// builder.Services.AddDbContext<MotoBikeContext>(...);
builder.Services.AddDbContext<MotoBikeContext>(opt =>
    opt.UseInMemoryDatabase("MotoDb"));
var app = builder.Build();

// ============ MIDDLEWARE ============
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // ⚠️ QUAN TRỌNG

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============ INITIALIZE IN-MEMORY DATA ============
InMemoryDataStore.Initialize();
Console.WriteLine("✓ In-Memory Data Store initialized");
Console.WriteLine($"✓ Products: {InMemoryDataStore.Products.Count}");
Console.WriteLine($"✓ Categories: {InMemoryDataStore.Categories.Count}");
Console.WriteLine($"✓ Users: {InMemoryDataStore.Users.Count}");

app.Run();