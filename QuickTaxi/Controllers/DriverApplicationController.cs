using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;
using System.Linq;

namespace QuickTaxi.Controllers
{
    [Authorize(Roles ="Driver, Passenger")]
    public class DriverApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DriverApplicationController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Driver, Passenger")]
        public async Task<IActionResult> RegisterDriver()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Profile", "Profile");
            }

            var model = new DriverRegistrationViewModel 
            {
                PhoneNumber = user.PhoneNumber,
                LastName = user.LastName,
                FirstName = user.FirstName,
            };
            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Driver, Passenger")]
        public async Task<IActionResult> RegisterDriver(DriverRegistrationViewModel model, IFormFile LicenseDocument, IFormFile InsuranceDocument, IFormFile ProfilePicture)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"❌ Validation Error: {error.ErrorMessage}"); // ✅ Logs each validation error
                }

                TempData["ErrorMessage"] = "Erreur dans le formulaire. Vérifiez les champs.";
                return View(model); // ✅ Returns the same form with errors
            }
            // 🔹 Debug : Afficher les fichiers reçus
            Console.WriteLine($"📷 Profile Picture: {(ProfilePicture != null ? ProfilePicture.FileName : "Aucun fichier reçu")}");
            Console.WriteLine($"📄 License Document: {(LicenseDocument != null ? LicenseDocument.FileName : "Aucun fichier reçu")}");
            Console.WriteLine($"📄 Insurance Document: {(InsuranceDocument != null ? InsuranceDocument.FileName : "Aucun fichier reçu")}");


            // 🔥 Vérifie que les fichiers sont bien chargés
            if (ProfilePicture == null || LicenseDocument == null || InsuranceDocument == null)
            {
                TempData["ErrorMessage"] = "Tous les fichiers doivent être téléchargés.";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Profile");
            }

            // Vérifier si le user est déjà un chauffeur
            var existingDriver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (existingDriver != null)
            {
                TempData["ErrorMessage"] = "Vous êtes déjà un chauffeur ou en attente pour le devenir.";
                return RedirectToAction("Profile");
            }

            var allowedImageExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
            var allowedDocExtensions = new List<string> { ".pdf", ".doc", ".docx" };

            
            // ✅ Save files and create the Driver record
            string profilePicturePath = await SaveFile(ProfilePicture);
            string licensePath = await SaveFile(LicenseDocument);
            string insurancePath = await SaveFile(InsuranceDocument);

            // Création du chauffeur
            var newDriver = new Driver
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                LicenseNumber = model.LicenseNumber,
                LicenseExpiration = model.LicenseExpiration,
                Address = model.Address,
                VehicleRegistrationNumber = model.VehicleRegistrationNumber,
                ProfilePictureUrl = profilePicturePath,  // ✅ Save File Path
                LicenseDocumentUrl = licensePath,  // ✅ Save File Path
                InsuranceDocumentUrl = insurancePath,
                IsApproved = false, // En attente d'approbation
                CreatedAt = DateTime.UtcNow,
                Status = "Offline"
            };


            Console.WriteLine($"First Name: {model.FirstName}");
            Console.WriteLine($"Last Name: {model.LastName}");
            Console.WriteLine($"Phone: {model.PhoneNumber}");
            Console.WriteLine($"License Number: {model.LicenseNumber}");
            Console.WriteLine($"License Expiration: {model.LicenseExpiration}");
            Console.WriteLine($"Vehicle Reg. Number: {model.VehicleRegistrationNumber}");
            Console.WriteLine($"License File Uploaded: {LicenseDocument != null}");
            Console.WriteLine($"Insurance File Uploaded: {InsuranceDocument != null}");


            _context.Drivers.Add(newDriver);
            await _context.SaveChangesAsync();

            // 🔹 Debug pour voir si le DriverId est bien généré
            Console.WriteLine($"✅ Driver enregistré avec ID = {newDriver.DriverId}");
            Console.WriteLine($"🚗 Véhicule : {model.VehicleMake} {model.VehicleModel}, Couleur: {model.VehicleColor}, Année: {model.VehicleYear}, Plaque: {model.VehicleRegistrationNumber}");

            // ✅ Enregistrer le véhicule
            var newVehicle = new Vehicle
            {
                VehicleId = Guid.NewGuid(),  // ✅ Keep GUID
                DriverId = newDriver.DriverId,
                Brand = model.VehicleMake,
                Model = model.VehicleModel,
                Color = model.VehicleColor,
                Year = model.VehicleYear,
                LicensePlate = model.LicenseNumber,
                VehicleRegistrationNumber = model.VehicleRegistrationNumber,
                InsuranceDocumentUrl = insurancePath,
                LicenseDocumentUrl = licensePath
            };

            Console.WriteLine($"🔍 Vérification: DriverId du véhicule = {newDriver.DriverId}");


            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            Console.WriteLine($"🚗 Véhicule enregistré avec ID = {newVehicle.VehicleId} pour DriverId = {newDriver.DriverId}");

            //TempData["SuccessMessage"] = "Votre demande a été envoyée pour validation.";
            return Redirect("https://localhost:44364/Profile/Profile");

        }

        // 📌 Fonction pour sauvegarder un fichier and Return Path
        private async Task<string> SaveFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);

            }

            return $"/uploads/{file.FileName}";

        }
    }
}
