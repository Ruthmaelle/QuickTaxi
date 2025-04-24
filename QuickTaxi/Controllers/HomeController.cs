using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickTaxi.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace QuickTaxi.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Console.WriteLine("===== Vérification dans HomeController =====");
            Console.WriteLine($"Utilisateur authentifié ? {User.Identity.IsAuthenticated}");
            Console.WriteLine($"Utilisateur connecté : {User.Identity.Name}");
            Console.WriteLine($"Type d'authentification : {User.Identity.AuthenticationType}");

            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("⚠️ Alerte : L'utilisateur n'est PAS authentifié après redirection !");
                Console.WriteLine("⚠️ Vérifie les cookies et sessions !");
            }
            else
            {
                var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                Console.WriteLine($"🔍 Rôles de l'utilisateur connecté : {string.Join(", ", roles)}");

                Console.WriteLine($"✅ L'utilisateur {User.Identity.Name} est bien connecté !");
            }

            if (User.IsInRole("Admin"))
            {
                Console.WriteLine("✅ L'utilisateur est bien Admin.");
            }
            else
            {
                Console.WriteLine("❌ L'utilisateur N'EST PAS Admin !");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
