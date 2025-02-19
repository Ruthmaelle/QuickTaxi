using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuickTaxi.Helpers;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;
using System.Threading.Tasks;

namespace QuickTaxi.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmsSender _smsSender;
        private readonly EmailSender _emailSender;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, SmsSender smsSender, EmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _smsSender = smsSender;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult VerifyCode(string email, string phone)
        {
            return View(new VerifyCodeViewModel { Email = email, PhoneNumber = phone} );
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            Console.WriteLine($"📩 DEBUG Register: Email={model.Email}, Phone={model.PhoneNumber}, Preferred={model.PreferredVerificationMethod}");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ✅ Generate a random 6-digit verification code
            var verificationCode = new Random().Next(100000, 999999).ToString();
            Console.WriteLine($"🔢 Generated Code: {verificationCode}");

            // ✅ Store the code temporarily (in session for now)
            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("PendingUserEmail", model.Email ?? "");
            HttpContext.Session.SetString("PendingUserPhone", model.PhoneNumber ?? "");

            // ✅ Send the code via email or SMS
            if (model.PreferredVerificationMethod == "email")
            {
                await _emailSender.SendVerificationCode(model.Email, verificationCode);
            }
            else
            {
                await _smsSender.SendSmsCode(model.PhoneNumber, verificationCode);
            }

            // ✅ Redirect user to verify the code
            return RedirectToAction("VerifyCode", new { isSms = (model.PreferredVerificationMethod == "sms") });


            /*Console.WriteLine("🚀 Register action has been called!"); // Debug: Vérifier si cette action est bien atteinte
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState is invalid!");
                // Debugging: Print validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }

                return View(model);
            }

            Console.WriteLine($"🔍 Received Password: {model.Password}");

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                Console.WriteLine("✅  User created succesfully");

                //verifier si le role exite sinon on l'ajoute
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                //Assigner le role a l'utilisateur
                await _userManager.AddToRoleAsync(user, model.Role);


                // Generate and store verification code
                string verificationCode = CodeGenerator.GenerateVerificationCode();
                user.VerificationCode = verificationCode;
                user.CodeExpiration = DateTime.UtcNow.AddMinutes(10); // Code valid for 5 min
                await _userManager.UpdateAsync(user);


                // 🔥 Envoyer le code en fonction du choix de l’utilisateur
                if (model.PreferredVerificationMethod == "email")
                {
                    Console.WriteLine($"📩 Sending email to {user.Email}");
                    await _emailSender.SendVerificationCode(user.Email, verificationCode);
                    return RedirectToAction("VerifyCode", new { email = user.Email });
                }
                else if (model.PreferredVerificationMethod == "sms")
                {
                    Console.WriteLine($"📱 Sending SMS to {user.PhoneNumber}");
                    await _smsSender.SendSmsCode(user.PhoneNumber, verificationCode);
                    return RedirectToAction("VerifyCode", new { phone = user.PhoneNumber });
                }
                else
                {
                    ModelState.AddModelError("", "Veuillez choisir une méthode de vérification.");
                    return View(model);
                }

                //Console.WriteLine($"📩 Verification code sent: {verificationCode}");

                // Send verification code (Email)
                //await EmailSender.SendVerificationCode(user.Email, verificationCode); // Function to implement

                // ✅ Redirect to verification page instead of home page

            }

            Console.WriteLine("❌ User creation failed!");
            // Debugging if registration failed
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);


            return View(model);

            */


        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string PreferredVerificationMethod)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 📌 Recherche de l'utilisateur dans la DB
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // 📩 Envoi du code de vérification selon le choix
                if (model.PreferredVerificationMethod == "email")
                {
                    Console.WriteLine($"📩 Envoi du code par EMAIL à {user.Email}");
                    TempData["Message"] = $"Un code a été envoyé à {user.Email}";
                }
                else
                {
                    Console.WriteLine($"📩 Envoi du code par SMS à {user.PhoneNumber}");
                    TempData["Message"] = $"Un code a été envoyé à {user.PhoneNumber}";
                }

                return RedirectToAction("VerifyCode");
            }

            ModelState.AddModelError("", "Email ou mot de passe incorrect.");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {

            Console.WriteLine($"🔍 DEBUG POST VerifyCode: Code={model.Code}");

            // ✅ Retrieve the stored verification code
            var storedCode = HttpContext.Session.GetString("VerificationCode");
            var email = HttpContext.Session.GetString("PendingUserEmail");
            var phone = HttpContext.Session.GetString("PendingUserPhone");

            Console.WriteLine($"🔍 Stored Code: {storedCode}, Email: {email}, Phone: {phone}");

            // ✅ Check if the entered code matches
            if (model.Code != storedCode)
            {
                Console.WriteLine("❌ Code incorrect !");
                ModelState.AddModelError("", "Code incorrect ou expiré.");
                return View(model);
            }

            // ✅ Create user now, since verification succeeded
            var user = new User
            {
                UserName = email ?? phone,
                Email = email,
                PhoneNumber = phone
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                Console.WriteLine("❌ ERREUR: Impossible de créer l'utilisateur.");
                return View(model);
            }

            // ✅ Clear session after successful registration
            HttpContext.Session.Remove("VerificationCode");
            HttpContext.Session.Remove("PendingUserEmail");
            HttpContext.Session.Remove("PendingUserPhone");

            // ✅ Log in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // ✅ Redirect to Home
            Console.WriteLine("🚀 Redirection vers Home !");
            return RedirectToAction("Index", "Home");


            /*Console.WriteLine($"📩 Vérification du code : Code={model.Code}, Email={model.Email}, Phone={model.PhoneNumber}");

            // 📌 On cherche l'utilisateur avec l'email OU le numéro de téléphone
            var pendingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => (model.Email != null && u.Email == model.Email)
                                       || (model.PhoneNumber != null && u.PhoneNumber == model.PhoneNumber));

            if (pendingUser == null)
            {
                Console.WriteLine("❌ Aucun utilisateur en attente de vérification !");
                ModelState.AddModelError("", "Utilisateur inconnu. Veuillez recommencer l'inscription.");
                return View(model);
            }

            // 📌 Vérification du code
            if (pendingUser.VerificationCode != model.Code || pendingUser.CodeExpiration < DateTime.UtcNow)
            {
                Console.WriteLine("❌ Code invalide ou expiré !");
                ModelState.AddModelError("", "Code incorrect ou expiré.");
                return View(model);
            }

            // ✅ Le code est correct : on enregistre l'utilisateur en activant son compte
            pendingUser.VerificationCode = null;
            pendingUser.CodeExpiration = null;

            var result = await _userManager.UpdateAsync(pendingUser);
            if (!result.Succeeded)
            {
                Console.WriteLine("❌ Erreur lors de l'activation du compte !");
                ModelState.AddModelError("", "Problème lors de l'activation de votre compte.");
                return View(model);
            }

            // 🔐 Connexion automatique après validation
            await _signInManager.SignInAsync(pendingUser, isPersistent: false);

            Console.WriteLine($"✅ Vérification réussie pour {pendingUser.UserName} ! Redirection...");
            return RedirectToAction("Index", "Home");

            */


            /*Console.WriteLine($"📩 Vérification (simulation) : Email={model.Email}, Phone={model.PhoneNumber}");

            User? user = null;

            // 📌 Chercher l'utilisateur selon l'email ou le numéro de téléphone
            if (!string.IsNullOrEmpty(model.Email))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            }
            else if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
            }

            // 📌 Si l'utilisateur n'existe pas, on le crée
            if (user == null)
            {
                user = new User
                {
                    UserName = model.Email ?? model.PhoneNumber,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FirstName = "Utilisateur", // Valeur temporaire
                    LastName = "Démo"
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    Console.WriteLine("❌ Erreur lors de la création du compte.");
                    ModelState.AddModelError("", "Erreur lors de la création du compte.");
                    return View(model);
                }
            }

            // ✅ Simulation : Vérification réussie
            Console.WriteLine($"✅ Bienvenue {user.FirstName} {user.LastName} !");
            TempData["WelcomeMessage"] = $"Bienvenue {user.FirstName} {user.LastName} ! 🎉";

            // 🔐 Connexion automatique
            await _signInManager.SignInAsync(user, isPersistent: false);

            // 🚀 Redirection vers la page d'accueil
            return RedirectToAction("Index", "Home"); 
            */

        }



    }
}
