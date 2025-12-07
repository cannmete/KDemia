using KDemia;
using KDemia.Data;
using KDemia.Mapper;
using KDemia.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Repository tanýtýmý
builder.Services.AddScoped(typeof(KDemia.Repositories.GenericRepository<>));

// DB Connection

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<KDemia.Data.AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// AutoMapper Ayarý
builder.Services.AddAutoMapper(typeof(UserProfile));

// Cooike Aktive etmek için
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";                 
        options.AccessDeniedPath = "/Auth/AccessDenied";    
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 60 dakika login süresi
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Kimlik 
app.UseAuthorization();  // Yetki

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
