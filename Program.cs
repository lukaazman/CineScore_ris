using CineScore.Data;
using CineScore.Models;
using CineScore.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=CineScore.db";
var overriddenDatabasePath = builder.Configuration["CINESCORE_DB_PATH"];
var azureHome = Environment.GetEnvironmentVariable("HOME");
string? dataProtectionKeysPath = null;

if (!string.IsNullOrWhiteSpace(overriddenDatabasePath))
{
    connectionString = $"Data Source={overriddenDatabasePath}";
}
else if (builder.Environment.IsProduction() && !string.IsNullOrWhiteSpace(azureHome))
{
    var dataDirectory = Path.Combine(azureHome, "site", "data");
    Directory.CreateDirectory(dataDirectory);
    connectionString = $"Data Source={Path.Combine(dataDirectory, "CineScore.db")}";

    dataProtectionKeysPath = Path.Combine(azureHome, "site", "data-protection-keys");
    Directory.CreateDirectory(dataProtectionKeysPath);
}

if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddDbContext<CineScoreContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<User>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CineScoreContext>();

builder.Services.AddScoped<K_OceniFilm>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CineScoreContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    await dbContext.Database.EnsureCreatedAsync();
    await CineScoreSeeder.SeedAsync(dbContext, userManager, roleManager);
}

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

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
