using Microsoft.EntityFrameworkCore;
using project.Data;  // чтобы видеть RecipesDbContext
using Microsoft.AspNetCore.Authentication.Cookies;
using project.Models;
using Microsoft.AspNetCore.Identity;
using project.Services;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("HF_TOKEN", builder.Configuration["HuggingFace:ApiKey"]);

// Google API ключи
// string apiKey = "AIzaSyBtDB8aE_0__NpcXeXOzLxaYymY6ht8XBI";
// string searchEngineId = "c74f19326b96749a9";

// DI контейнер
// builder.Services.AddSingleton(new GoogleSearchService(apiKey, searchEngineId));
// builder.Services.AddScoped<AIReplyService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// builder.Services.AddDbContext<RecipesDbContext>(options =>
//     options.UseMySql(
//         builder.Configuration.GetConnectionString("DefaultConnection"),
//         new MySqlServerVersion(new Version(8, 0, 412)) // укажи свою версию MySQL
//     ));

builder.Services.AddDbContext<RecipesDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 412)) // или версия твоего MySQL
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acc/Login"; // куда редиректить, если не авторизован
        options.LogoutPath = "/Acc/Logout";
        options.SlidingExpiration = false;

        // остальные настройки
        options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<project.Services.RecipeService>();

builder.Services.AddScoped<project.Services.CategoryService>();

builder.Services.AddSession(); // регистрируем сессии

builder.Services.AddControllers();

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

app.UseStaticFiles();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=title_page}/{id?}");

app.Run();