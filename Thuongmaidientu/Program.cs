using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Thuongmadientu.Helpers;
using Thuongmaidientu.Data;
using Thuongmaidientu.Helpers;
using Thuongmaidientu.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MyeStoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("eStore"));
});

builder.Services.AddAuthentication()
    .AddCookie("SellerCookie", options =>
    {
        options.LoginPath = "/NhanVien/DangNhap";
        options.Cookie.Name = "SellerCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.MaxAge = null; // mất khi tắt trình duyệt
    })
    .AddCookie("CustomerCookie", options =>
    {
        options.LoginPath = "/KhachHang/DangNhap";
        options.Cookie.Name = "CustomerCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.MaxAge = null; // mất khi tắt trình duyệt
    });


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAutoMapper(typeof(AutoMapperProFile));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/KhachHang/DangNhap";
        options.AccessDeniedPath = "/AccessDenied";
    });

// PaypalClient singleton
builder.Services.AddSingleton(x => new PaypalClient(
    builder.Configuration["PaypalOptions:AppId"],
    builder.Configuration["PaypalOptions:AppSecret"],
    builder.Configuration["PaypalOptions:Mode"]
));

builder.Services.AddSingleton<IVnPayService, VnPayService>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route SEO friendly
app.MapControllerRoute(
    name: "ProductDetails",
    pattern: "product/{id}/{slug?}",
    defaults: new { controller = "Hanghoa", action = "Detail" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
