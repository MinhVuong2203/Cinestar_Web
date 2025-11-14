using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web.Data;
using Web.Filters;
using Web.Service;
using Web.Models.Configuration;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorOptions(options =>
{
    options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // GDPR compliance
    options.Cookie.Name = ".CinestarWeb.Session";
});

// Configure PayOS Settings
builder.Services.Configure<PayOsSettings>(builder.Configuration.GetSection("PayOS"));

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        //options.Cookie.MaxAge = null;
        options.Cookie.Name = "CinestarAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddDbContext<CineStarContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CineStarDb")),
    ServiceLifetime.Scoped);

// Đăng ký Service
builder.Services.AddScoped<ICinemaBranchService, CinemaBranchService>();
builder.Services.AddScoped<IMovieService_Cus, MovieService_Cus>();
builder.Services.AddScoped<IShowTimeService, ShowTimeService>();
builder.Services.AddScoped<ILogin, Login>();
builder.Services.AddScoped<IPayOsService, PayOsService>();
builder.Services.AddHostedService<WorkShiftStatusUpdater>();
builder.Services.AddHostedService<SeatCleanupBackgroundService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPurchaseHistoryService, PurchaseHistoryService>();

// Đăng ký tất cả service trong namespace Web.Areas.Admin.Services
var adminAssembly = typeof(Web.Areas.Admin.Controllers.HomeController).Assembly;
builder.Services.Scan(scan => scan
    .FromAssemblies(adminAssembly)
        .AddClasses(classes => classes.InNamespaces("Web.Areas.Admin.Service"))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

// Đăng ký Filters
builder.Services.AddControllersWithViews(option =>
{
    option.Filters.Add<LoadCinemaBranchesAttribute>();
});

// Đăng ký SignalR 
builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SeatHub>("/seatHub");
app.UseSession();

// Cấu hình router - THÊM ROUTE CHO CÁC ROLE
app.MapControllerRoute(
    name: "admin_area",
    pattern: "admin/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "employee_sales",
    pattern: "employee-sales/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "employee_technician",
    pattern: "employee-technician/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "employee_movies",
    pattern: "employee-movies/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();