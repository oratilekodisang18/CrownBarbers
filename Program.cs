using CrownBarbers;
using Microsoft.EntityFrameworkCore;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
var dir = Path.Combine(b.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dir);
b.Services.AddDbContext<AppDb>(o => o.UseSqlite($"Data Source={Path.Combine(dir, "crown.db")}"));
var app = b.Build();
using (var s = app.Services.CreateScope()) s.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreated();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler(e => e.Run(c => c.Response.WriteAsync("Something went wrong. Please go back and try again.")));
app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();
app.Run();