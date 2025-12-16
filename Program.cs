using KDemia;
using KDemia.Data;
using KDemia.Mapper;
using KDemia.Repositories;
using KDemia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Repository tanýtýmý
builder.Services.AddScoped(typeof(KDemia.Repositories.GenericRepository<>));

// DB Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<KDemia.Data.AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- IDENTITY BAÞLANGIÇ ---
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Þifre Kurallarý
    options.Password.RequireDigit = true; // Sayý olsun
    options.Password.RequiredLength = 4;  // En az 4 karakter
    options.Password.RequireNonAlphanumeric = false; // Sembol zorunlu deðil (!,*, vs.)
    options.Password.RequireUppercase = false; // Büyük harf zorunlu deðil
    options.Password.RequireLowercase = false; // Küçük harf zorunlu deðil
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie Ayarlarý
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});
// --- IDENTITY BÝTÝÞ ---


// AutoMapper Ayarý
builder.Services.AddAutoMapper(typeof(UserProfile));

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

app.UseAuthentication(); // Kimlik  
app.UseAuthorization();  // Yetki
app.MapHub<KDemia.Hubs.NotificationHub>("/notificationHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();