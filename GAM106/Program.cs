using GAM106.Data;
using GAM106.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add Razor Pages (for Identity UI)
builder.Services.AddRazorPages();

// Configure EF Core with SQL Server (LocalDB)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\MSSQLLocalDB;Database=GAM106Db;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed sample data (levels, regions, questions, admin user)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        // apply any pending migrations
        db.Database.Migrate();

        // Seed GameLevels
        if (!db.GameLevels.Any())
        {
            db.GameLevels.AddRange(
                new GAM106.Models.GameLevel { Title = "Easy", Description = "Easy level" },
                new GAM106.Models.GameLevel { Title = "Medium", Description = "Medium level" },
                new GAM106.Models.GameLevel { Title = "Hard", Description = "Hard level" }
            );
            db.SaveChanges();
        }

        // Seed Regions
        if (!db.Regions.Any())
        {
            db.Regions.AddRange(
                new GAM106.Models.Region { Name = "North" },
                new GAM106.Models.Region { Name = "South" },
                new GAM106.Models.Region { Name = "Central" }
            );
            db.SaveChanges();
        }

        // Seed Questions (simple examples)
        if (!db.Questions.Any())
        {
            var easyLevel = db.GameLevels.First();
            db.Questions.AddRange(
                new GAM106.Models.Question
                {
                    ContentQuestion = "What is 2 + 2?",
                    Answer = "4",
                    Option1 = "1",
                    Option2 = "2",
                    Option3 = "3",
                    Option4 = "4",
                    LevelId = easyLevel.LevelId
                },
                new GAM106.Models.Question
                {
                    ContentQuestion = "What color is the sky?",
                    Answer = "Blue",
                    Option1 = "Red",
                    Option2 = "Green",
                    Option3 = "Blue",
                    Option4 = "Yellow",
                    LevelId = easyLevel.LevelId
                }
            );
            db.SaveChanges();
        }

        // Seed Identity admin user & role
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var adminRole = "Admin";
        if (!roleManager.RoleExistsAsync(adminRole).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(adminRole)).GetAwaiter().GetResult();
        }

        var adminEmail = "admin@example.com";
        var admin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (admin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true
            };
            var createResult = userManager.CreateAsync(adminUser, "Admin@123").GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, adminRole).GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
