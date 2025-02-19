using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using QuickTaxi.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuickTaxi.Helpers;
using QuickTaxi.helpers;

var builder = WebApplication.CreateBuilder(args);

// 📌 Récupération des variables d'environnement (Pour la sécurité)
var smtpPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD");
var twilioSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
var twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

// 🛠 Configuration de la base de données MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 23))));

// Configuration de Identity pour gérer les utilisateurs
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// 📌 Configuration des services Email et Twilio
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<TwiolioSettings>(builder.Configuration.GetSection("Twilio"));

// 📌 Injection des services
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddSingleton<SmsSender>();

var app = builder.Build();

// ✅ Initialisation des rôles et admin après la construction de `app`
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    string[] roles = { "Passenger", "Driver", "Admin" };

    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role).Result)
        {
            roleManager.CreateAsync(new IdentityRole(role)).Wait();
        }
    }
}

// Middleware Configuration
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
