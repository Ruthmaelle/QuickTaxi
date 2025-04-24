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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Stripe;



var builder = WebApplication.CreateBuilder(args);

// 📌 Récupération des variables d'environnement (Pour la sécurité)
var smtpPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD");
var twilioSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
var twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

// 🛠 Configuration de la base de données MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

/*builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()  // ✅ Ajout du support des rôles
    .AddEntityFrameworkStores<ApplicationDbContext>();
*/

// Configuration de Identity pour gérer les utilisateurs
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    /*options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;*/
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

builder.Services.AddTransient<EmailSender>();
builder.Services.AddTransient<SmsSender>();

builder.Services.AddHostedService<RideReminderService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";  // 🚀 Assure-toi que c'est bien cette URL !
    options.AccessType = "offline";  // 🔥 Permet de désactiver la connexion automatique
    options.SaveTokens = true;  // 🔥 Garde le token Google mais empêche l'auto-login
    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // Expire après 30 minutes
    options.SlidingExpiration = true; // Renouvelle automatiquement si actif
    options.LoginPath = "/Auth/Login"; // Redirige vers la page de connexion si expiré
    options.LogoutPath = "/Auth/Logout"; // S'assure que la déconnexion est bien gérée
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Redirige si accès interdit
    options.Cookie.HttpOnly = true; // Sécurise le cookie
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Sécurise avec HTTPS

    /*/options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Auth/ExternalLogin"))
        {
            context.Response.Redirect("/Auth/Login");
        }
        return Task.CompletedTask;
    };*/
});

builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var stripeSettings = builder.Configuration.GetSection("Stripe");
StripeConfiguration.ApiKey = stripeSettings["SecretKey"];

builder.Services.AddScoped<ReceiptGenerator>();

var app = builder.Build();
app.UseSession(); // Enable session middleware


// ✅ Initialisation des rôles et admin après la construction de `app`
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var services = scope.ServiceProvider;
    /*await SeedData.Initialize(services);
    await SeedData.CreateOrUpdateAdmin(services);*/

    string[] roles = { "Passenger", "Driver", "Admin" };

    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role).Result)
        {
            roleManager.CreateAsync(new IdentityRole(role)).Wait();
        }
    }

    // Vérifier si le rôle Admin existe
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
        Console.WriteLine("✅ Rôle Admin créé !");
    }

    // Vérifier si l'utilisateur Admin existe
    var adminEmail = "admin0example@gmail.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Mal",
            LastName = "D",
            PhoneNumber ="4383683837",

            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            Console.WriteLine("✅ Admin ajouté au rôle Admin !");
        }
        else
        {
            Console.WriteLine("❌ Erreur lors de la création de l'Admin !");
        }
    }
    else
    {
        Console.WriteLine("✅ L'Admin existe déjà !");
    }
}

// Middleware Configuration
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // ✅ Ensures session is enabled
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Error/AccessDenied");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
