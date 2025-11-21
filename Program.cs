using GAM106_LAB.Data;
using GAM106_LAB.Models;
using GAM106_LAB.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add CORS policy to allow requests from any origin for development/testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DB provider dynamically based on connection string
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
if (defaultConn.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) || defaultConn.Trim().EndsWith('.' + "db", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(defaultConn));
    Console.WriteLine("Using SQLite provider (DefaultConnection).");
}
else if (!string.IsNullOrWhiteSpace(defaultConn))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(defaultConn));
    Console.WriteLine("Using SQL Server provider (DefaultConnection).");
}
else
{
    // Fallback to SQLite file if no connection string provided
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=GAM106-LAB.db"));
    Console.WriteLine("No DefaultConnection provided — falling back to SQLite GAM106-LAB.db");
}

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// register email service
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddSwaggerGen(c =>
{
   c.SwaggerDoc("v1", new() { Title = "GAM106_LAB API", Version = "v1" });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Send a test email on startup if settings present (optional)
try
{
    var emailService = app.Services.GetRequiredService<IEmailService>();
    var emailSettings = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailSettings>>().Value;
    if (!string.IsNullOrEmpty(emailSettings.SenderEmail))
    {
        // Send a test email (customize as needed)
        await emailService.SendEmailAsync(emailSettings.SenderEmail, "Hola Bro", "Gửi email thành công.");
        Console.WriteLine("OK 10 Diem.");
    }
    else
    {
        Console.WriteLine("Email settings are not configured. Skipping test email.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error sending test email: " + ex.Message);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GAM106_LAB API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

// Enable CORS globally (must be between UseRouting and UseAuthorization)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Map attribute-routed controllers (e.g., controllers using [Route])
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("Unhandled exception during application run:");
    Console.WriteLine(ex.ToString());
    throw;
}