using Microsoft.EntityFrameworkCore;
using project.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using project.Models;
using Microsoft.AspNetCore.Identity;
using project.Services;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("HF_TOKEN", builder.Configuration["HuggingFace:ApiKey"]);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RecipesDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 412))
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acc/Login";
        options.LogoutPath = "/Acc/Logout";
        options.SlidingExpiration = false;

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

builder.Services.AddSession();

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