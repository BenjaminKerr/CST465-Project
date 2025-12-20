// Quantum algorithm visualization web application
// Main goal is to make Grover's Search Algorithm more understandable through interactive visualizations.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CST465_project.Data;
using CST465_project.Models;
using CST465_project.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var identityBuilder = builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false);
identityBuilder.AddRoles<IdentityRole>();
identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Memory cache for repository caching
builder.Services.AddMemoryCache();

// Repository registrations: EF implementation + cached decorator
builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<ICommentRepository, CachedCommentRepository>();

builder.Services.AddScoped<VisualizationRepository>();
builder.Services.AddScoped<IVisualizationRepository, CachedVisualizationRepository>();

// Map attribute-routed API controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Ensure authentication middleware is enabled before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Ensure attribute-routed controllers (API) are mapped
app.MapControllers();

// Seed admin user/role if configured
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    try
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var adminEmail = configuration["AdminUser:Email"];
        var adminPassword = configuration["AdminUser:Password"];
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Admin seeding: email={Email}", adminEmail);
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>>();
            var roleExists = roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult();
            if (!roleExists)
            {
                var createRoleResult = roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin")).GetAwaiter().GetResult();
                if (createRoleResult.Succeeded)
                {
                    logger.LogInformation("Admin role created.");
                }
                else
                {
                    logger.LogWarning("Failed to create Admin role: {Errors}", string.Join(';', createRoleResult.Errors.Select(e => e.Description)));
                }
            }

            var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var r = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
                if (r.Succeeded)
                {
                    var addToRoleResult = userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Admin user created and added to Admin role.");
                    }
                    else
                    {
                        logger.LogWarning("Admin user created but failed to add to role: {Errors}", string.Join(';', addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogWarning("Failed to create admin user: {Errors}", string.Join(';', r.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                var inRole = userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                if (!inRole)
                {
                    var addToRoleResult = userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Existing admin user added to Admin role.");
                    }
                    else
                    {
                        logger.LogWarning("Failed to add existing user to Admin role: {Errors}", string.Join(';', addToRoleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists and is in Admin role.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during admin seeding");
        // don't block startup on seed errors
    }
}

app.Run();
