using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using WallpaperPortal.Infrastructure;
using WallpaperPortal.Middlewares;
using WallpaperPortal.Models;
using WallpaperPortal.Persistance;
using WallpaperPortal.Services;
using WallpaperPortal.Services.Abstract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration["MySQLConnection:ConnectionString"];

var serverVersion = new MySqlServerVersion(new Version(8, 0, 34));

builder.Services.AddDbContext<ApplicationContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(connectionString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);

builder.Services.AddIdentity<User, IdentityRole>(
    options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Authorization/Login");
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISitemapGenerator, SitemapGenerator>();

builder.Services.AddControllersWithViews();

await RoleInitializer.CreateRoles(builder.Services.BuildServiceProvider(), builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=File}/{action=Files}/{id?}");

string filesFolderPath = Path.Combine(app.Environment.WebRootPath, "Uploads/Files");
if (!Directory.Exists(filesFolderPath))
{
    Directory.CreateDirectory(filesFolderPath);
}

string previewsFolderPath = Path.Combine(app.Environment.WebRootPath, "Uploads/Previews");
if (!Directory.Exists(previewsFolderPath))
{
    Directory.CreateDirectory(previewsFolderPath);
}

string ProfileImagesPath = Path.Combine(app.Environment.WebRootPath, "Uploads/ProfileImages");
if (!Directory.Exists(ProfileImagesPath))
{
    Directory.CreateDirectory(ProfileImagesPath);
}

app.Run();
