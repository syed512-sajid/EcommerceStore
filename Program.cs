using EcommerceStore.Data;
using EcommerceStore.Models;
using EcommerceStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// DATABASE - SQLite (LOCAL)
// ===============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

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
// EMAIL
// ===============================
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<BackgroundEmailService>();

var app = builder.Build();

// ===============================
// MIGRATION + ROLES + ADMIN SEED
// ===============================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Run migrations
    await db.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // ✅ CREATE ADMIN ROLE
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    // ✅ CREATE CUSTOMER ROLE (IMPORTANT!)
    if (!await roleManager.RoleExistsAsync("Customer"))
        await roleManager.CreateAsync(new IdentityRole("Customer"));

    // ✅ SEED ADMIN USER
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