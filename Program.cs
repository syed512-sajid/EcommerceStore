using EcommerceStore.Data;
using EcommerceStore.Models;
using EcommerceStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ===============================
// BUILD APP
// ===============================
var builder = WebApplication.CreateBuilder(args);

// ===============================
// DATABASE - SQLite (Railway Volume Safe)
// ===============================

// Railway volume path (set in Railway Variables: DB_PATH=/data)
var dbRoot = Environment.GetEnvironmentVariable("DB_PATH")
             ?? builder.Environment.ContentRootPath;

// Ensure directory exists
Directory.CreateDirectory(dbRoot);

// Full database file path inside the volume
var dbPath = Path.Combine(dbRoot, "Ecommerce.db");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);

// ===============================
// IDENTITY
// ===============================
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ===============================
// SESSION
// ===============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===============================
// MVC
// ===============================
builder.Services.AddControllersWithViews();

// ===============================
// EMAIL SERVICE
// ===============================
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<BackgroundEmailService>();

var app = builder.Build();

// ===============================
// APPLY MIGRATIONS + SEED ROLES + ADMIN
// ===============================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    // ✅ Apply pending migrations safely
    // This will NOT delete existing data, only update schema
    await db.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Create roles if they don't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("Customer"))
        await roleManager.CreateAsync(new IdentityRole("Customer"));

    // Seed admin user
    string adminEmail = "sajidabbas6024@gmail.com";
    string adminPassword = "Admin@6024";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// ===============================
// MIDDLEWARE
// ===============================
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

// ===============================
// ROUTES
// ===============================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
