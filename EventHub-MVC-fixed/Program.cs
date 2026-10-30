using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MVC + chong CSRF cho moi form POST
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Tang DB (Repository) - moi class lam 1 viec
builder.Services.AddSingleton<Db>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VenueRepository>();
builder.Services.AddScoped<BookingRepository>();

// Dang nhap bang cookie + phan quyen theo role
builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromDays(7);
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Tao du lieu mau lan dau (bo qua neu chua co DB)
try
{
    using var scope = app.Services.CreateScope();
    DbSeeder.Seed(scope.ServiceProvider.GetRequiredService<UserRepository>(),
                  scope.ServiceProvider.GetRequiredService<VenueRepository>());
}
catch (Exception ex)
{
    app.Logger.LogWarning("Seed skipped (da chay database/eventhub.sql chua?): {Message}", ex.Message);
}

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
