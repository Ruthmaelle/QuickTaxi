using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using System.Security.Claims;
using QuickTaxi.Helpers;
using System.Numerics;

namespace QuickTaxi.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmsSender _smsSender;
        private readonly EmailSender _emailSender;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, SmsSender smsSender, EmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _smsSender = smsSender;
            _emailSender = emailSender;
        }

        private async Task SendApprovalNotification(User user)
        {
            string message = $"Bonjour {user.FirstName}, votre demande de chauffeur sur QuickTaxi a été approuvée ! 🎉 Vous pouvez maintenant commencer à conduire.";
            await _smsSender.SendSmsCode(user.PhoneNumber, message);
        }

        private async Task SendRejectionNotification(User user)
        {
            string message = $"Bonjour {user.FirstName}, nous sommes désolés de vous informer que votre demande pour devenir chauffeur sur QuickTaxi a été refusée.";
            await _smsSender.SendSmsCode(user.PhoneNumber, message);
        }


        //1- AFFICHER LE PROFIL DU USER
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var pendingDrivers = new List<Driver>();   
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var totalPoints = await _context.Rewards
                .Where(r => r.UserId == user.Id)
                .SumAsync(r => r.Points);

            ViewBag.TotalPoints = totalPoints;
            Console.WriteLine($"📌 Rôles actuels de l'utilisateur : {string.Join(", ", roles)}");
            string userRole = roles.FirstOrDefault() ?? "Passenger";
            if (user == null)
            {
                await _signInManager.SignOutAsync(); // 🔥 Déconnecte l'utilisateur s'il n'est pas reconnu
                return RedirectToAction("Login", "Auth");
            }

            // ✅ Vérifie que l'ID utilisateur est valide
            if (string.IsNullOrEmpty(user.Id))
            {
                Console.WriteLine("❌ User.Id est NULL ou vide !");
                return RedirectToAction("Login", "Auth");
            }

            if (User.IsInRole("Admin")) // ✅ Vérifie dans roles plutôt que User.IsInRole()
            {
                pendingDrivers = await _context.Drivers
                   .Include(d => d.User) // ✅ Ajout pour récupérer l'User lié
                   .Include(d => d.Vehicle) 
                   .Where(d => d.IsApproved == null || d.IsApproved == false)
                   .ToListAsync();

                /*pendingDrivers = await _context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Vehicle)
                    .Where(d => d.IsApproved == null || d.IsApproved == false)
                    .Select(d => new Driver
                    {
                        DriverId = d.DriverId,
                        UserId = d.UserId,
                        IsApproved = d.IsApproved ?? false, // 🔥 Gère NULL
                        ProfilePicture = d.ProfilePicture,
                    })
                    .ToListAsync();*/
            }

            Console.WriteLine($"🔍 Vérification : UserId = {user?.Id}");



            // ✅ Vérification et récupération du chauffeur (gère DBNull)
            /*var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Vehicle)
                .Where(d => d.UserId == user.Id)
                .Select(d => new
                {
                    d.DriverId,
                    d.UserId,
                    IsApproved = d.IsApproved ?? false // ✅ Si NULL, il sera false
                })
                .FirstOrDefaultAsync();*/

            var driver = await _context.Drivers
            .Include(d => d.User)
            .Include(d => d.Vehicle) // ✅ Ensure Vehicle is fully loaded
            .FirstOrDefaultAsync(d => d.UserId == user.Id);


            bool hasPendingRequest = driver?.IsApproved == false; // ✅ Vérifie si une demande est en attente
            bool isAlreadyDriver = driver?.IsApproved  == true; // ✅ Vérifie si le chauffeur est déjà approuvé

            Vehicle vehicle = null;

            if (driver != null)
            {
                vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.DriverId == driver.DriverId);
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName ?? "", // ✅ Évite la conversion DBNull -> string
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Roles = roles.ToList(),
                HasPendingDriverRequest = hasPendingRequest,
                IsAlreadyDriver = isAlreadyDriver,
                PendingDrivers = pendingDrivers ?? new List<Driver>(),
                Vehicle = vehicle //pass vehicle to view model
            };


            Console.WriteLine($"🔍 User Roles: {string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

            Console.WriteLine("errrrrrrrrrrr");
            return View(model);

        }


        //2- AFFICHER LE FORMULAIRE D'EDITION
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync (User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            return View(model);
        }

        //3-Traiter la modification du profil
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            ModelState.Remove("Vehicule");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 🛑 Empêcher la modification de l'email
            if (model.Email != user.Email)
            {
                ModelState.AddModelError("", "L'email ne peut pas être modifié.");
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Profil mis à jour avec succès !";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestDriver()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Profile");
            }

            // ✅ Vérification de l'ID utilisateur
            if (string.IsNullOrEmpty(user.Id))
            {
                TempData["ErrorMessage"] = "Erreur : l'identifiant utilisateur est invalide.";
                return RedirectToAction("Profile");
            }
            Console.WriteLine($"🔍 Vérification : UserId = {user.Id}");


            //verifier si le user est deja un chauffeur
            Console.WriteLine($"🔍 Debug - Recherche du chauffeur : UserId = {user.Id}");
            var existingDriver = await _context.Drivers
                .Where(d => d.UserId == user.Id)
                .Select(d => new
                {
                    d.DriverId,
                    d.UserId,
                    IsApproved = d.IsApproved ?? false // Si NULL, on met false
                })
                .FirstOrDefaultAsync();
            Console.WriteLine($"📝 Résultat : {existingDriver}");
            if (existingDriver != null)
            {
                if (existingDriver.IsApproved == false)
                {
                    TempData["ErrorMessage"] = "Votre demande est déjà en attente.";
                    return RedirectToAction("Profile");
                }
                if (existingDriver.IsApproved == true)
                {
                    TempData["ErrorMessage"] = "Vous êtes déjà un chauffeur.";
                    return RedirectToAction("Profile");
                }
            }
            // ✅ Redirect the user to complete their driver registration
            return RedirectToAction("RegisterDriver", "DriverApplication");
        }

        public async Task<bool> HasPendingDriverRequest(string userId)
        {
            return await _context.Drivers.AnyAsync(d => d.UserId == userId && d.IsApproved.GetValueOrDefault() == false);
        }


        [Authorize(Roles = "Admin")] // Seuls les Admins voient et approuvent les demandes
        public async Task<IActionResult> DriverRequests()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Auth");
            }

            Console.WriteLine($"🔍 Vérification User: {user.Email}");
            Console.WriteLine($"🔍 ID: {user.Id}");

            /// 🔍 Vérification des rôles ASP.NET
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            Console.WriteLine($"🔍 Admin détecté dans UserManager ? {isAdmin}");

            if (!isAdmin)
            {
                Console.WriteLine("❌ L'utilisateur n'a PAS le rôle Admin !");
                return RedirectToAction("Login", "Auth");
            }
            /* if (!User.IsInRole("Admin"))
             {
                 Console.WriteLine("⚠️ ERREUR : L'utilisateur actuel n'est pas Admin !");
                 return RedirectToAction("AccessDenied", "Auth"); // 🔥 Redirige pour vérifier
             }*/

            Console.WriteLine("✅ Admin détecté, chargement des chauffeurs en attente...");

            var pendingDrivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Vehicle)
                .Where(d => d.IsApproved == null || d.IsApproved == false)
                /*.Select(d => new
                {
                    d.DriverId,
                    d.UserId,
                    IsApproved = d.IsApproved ?? false,  //si c'est null on renvoie false
                    DateOfBirth = d.DateOfBirth == null ? (DateTime?)null : d.DateOfBirth, //gere les null'
                    LicenseExpiration = d.LicenseExpiration == null ? (DateTime?)null : d.LicenseExpiration

                })*/
                .ToListAsync();
                //.Include(d => d.User) // Charger les infos utilisateur
                //.ToListAsync();



            return View("~/Views/Driver/DriverRequests.cshtml", pendingDrivers);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveDriver(Guid driverId)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (driver == null)
            {
                TempData["ErrorMessage"] = "Chauffeur introuvable.";
                return RedirectToAction("DriverRequests");
                //return NotFound();
            }

            // ✅ Vérifier si le véhicule est bien enregistré
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.DriverId == driverId);
            if (vehicle == null)
            {
                TempData["ErrorMessage"] = "Impossible d'approuver ce chauffeur sans véhicule enregistré.";
                return RedirectToAction("DriverRequests");
            }

            if (driver.IsApproved == true)
            {
                TempData["ErrorMessage"] = "⚠️ Ce chauffeur est déjà approuvé.";
                return RedirectToAction("DriverRequests");
            }

            //marque approved
            driver.IsApproved = true;
            driver.Status = "Offline"; //mise a jour du statut, offline par defaut
            _context.Drivers.Update(driver);
            await _context.SaveChangesAsync();

            //modifier le role du User en "Driver"
            var user = await _userManager.FindByIdAsync(driver.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ Impossible de trouver l'utilisateur associé.";
                return RedirectToAction("DriverRequests");
            }

            //ajouter le role "DRIVER"
            if(user != null)
            {
                if(!await _userManager.IsInRoleAsync(user, "Driver"))
                {
                    await _userManager.AddToRoleAsync(user, "Driver");
                    await SendApprovalNotification(user);
                }
            }
            await SendApprovalNotification(user);


            TempData["SuccessMessage"] = "Le chauffeur a été approuvé avec succès.";
            return RedirectToAction("DriverRequests");
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RejectDriver(Guid driverId)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (driver == null)
            {
                TempData["ErrorMessage"] = "Chauffeur introuvable.";
                return RedirectToAction("DriverRequests");
            }
            // Trouver l'utilisateur associé et lui Envoyer une notification au user
            var user = await _userManager.FindByIdAsync(driver.UserId);
            if (user != null && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                // ✅ Vérifier si l'utilisateur a déjà le rôle "Driver" par err et le supprimer
                if (await _userManager.IsInRoleAsync(user, "Driver"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Driver");
                }
                await SendRejectionNotification(user);
            }

            // Supprimer la demande du chauffeur
            _context.Drivers.Remove(driver);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "La demande du chauffeur a été refusée.";
            return RedirectToAction("DriverRequests");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string roleToRemove)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Profile");
            }

            // ✅ Vérifier que le rôle à supprimer n'est pas null ou vide
            if (string.IsNullOrEmpty(roleToRemove))
            {
                TempData["ErrorMessage"] = "Aucun rôle spécifié pour suppression.";
                return RedirectToAction("Profile");
            }

            // ✅ Vérifier que l'utilisateur possède bien ce rôle
            if (!await _userManager.IsInRoleAsync(user, roleToRemove))
            {
                TempData["ErrorMessage"] = $"Le rôle {roleToRemove} n'existe pas pour cet utilisateur.";
                return RedirectToAction("Profile");
            }

            //suppression du role
            var result = await _userManager.RemoveFromRoleAsync(user, roleToRemove);
            if(result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Le rôle {roleToRemove} a été supprimé avec succès.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Erreur lors de la suppression du rôle.";
            }

            return RedirectToAction("Profile");
        }   

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Profile");
            }

            //supprimer le user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Erreur lors de la suppression du compte.";
                return RedirectToAction("Profile");
            }

            
            await _signInManager.SignOutAsync();

            TempData["SuccessMessage"] = "Votre compte a été supprimé avec succès.";    
            return RedirectToAction("Index", "Home");
        }


    }
}
