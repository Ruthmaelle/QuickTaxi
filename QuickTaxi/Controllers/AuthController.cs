using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuickTaxi.Data;
using QuickTaxi.Helpers;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuickTaxi.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context; // Ajout de _context

        public AuthController(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              RoleManager<IdentityRole> roleManager,
                              ApplicationDbContext context,
                              SmsSender smsSender,
                              EmailSender emailSender) // Injecter context
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context; // Assigner à _context
            _smsSender = smsSender;
            _emailSender = emailSender;
        }


        private string GenerateVerificationCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SmsSender _smsSender;
        private readonly EmailSender _emailSender;


        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            Console.WriteLine("🚪 Déconnexion en cours...");

            // Supprimer les connexions (Google, Facebook, Email)
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Suppression des cookies d'authentification
            Response.Cookies.Delete("AspNetCore.Identity.Application");
            Response.Cookies.Delete("AspNetCore.Cookies");
            Response.Cookies.Delete("AspNetCore.AntiForgery");
            Response.Cookies.Delete(".AspNetCore.Identity.External");
            Response.Cookies.Delete(".AspNetCore.Google");

            Console.WriteLine("✅ Cookies supprimés avec succès.");

            return RedirectToAction("Index", "Home");

            // 🔴 Redirection forcée vers Google Logout
            /*var googleLogoutUrl = "https://accounts.google.com/Logout";
            return Redirect(googleLogoutUrl);*/
        }

        [HttpPost]
        public async Task<IActionResult> ForceLogout()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("AspNetCore.Identity.Application"); // Supprime manuellement le cookie d'authentification
            Response.Cookies.Delete("AspNetCore.Cookies"); // Supprime aussi ce cookie s'il est présent
            Response.Cookies.Delete("AspNetCore.AntiForgery"); // Supprime le token CSRF au cas où

            Console.WriteLine("✅ Déconnexion forcée !");
            return RedirectToAction("Login");
        }



        [HttpGet]
        public IActionResult VerifyCode(string email, string phone)
        {
            return View(new VerifyCodeViewModel { Email = email, PhoneNumber = phone} );
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            Console.WriteLine($"📌 DEBUG: Register started");


            //1- verification du modele
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"❌ Model state is invalid");
                foreach (var error in ModelState.Values.SelectMany(x => x.Errors))
                {
                    Console.WriteLine($"❌ Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            Console.WriteLine($"📌 FirstName: {model.FirstName}");
            Console.WriteLine($"📌 LastName: {model.LastName}");
            Console.WriteLine($"📌 Email: {model.Email}");
            Console.WriteLine($"📌 PhoneNumber: {model.PhoneNumber}");
            Console.WriteLine($"📌 PreferredVerificationMethod: {model.PreferredVerificationMethod}");

            // 2️⃣ Vérification des champs obligatoires
            if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.PhoneNumber))
            {
                Console.WriteLine("❌ ERROR: Email and PhoneNumber are both empty!");
                return View(model);
            }

            // 1️⃣ **Vérifier si l'utilisateur existe déjà dans la base de données (AVEC SON EMAIL)
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                Console.WriteLine("❌ ERROR: Un utilisateur avec cet email existe déjà !");
                ModelState.AddModelError("", "Un compte avec cet email existe déjà.");
                return View(model);
            }
            // Si on veut aussi vérifier par numéro de téléphone
            /*var existingPhoneUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (existingPhoneUser != null)
            {
                Console.WriteLine("❌ ERROR: Un utilisateur avec ce numéro de téléphone existe déjà !");
                ModelState.AddModelError("", "Un compte avec ce numéro de téléphone existe déjà.");
                return View(model);
            }*/

            // ✅ Generate a random verification code
            var verificationCode = GenerateVerificationCode();


            // ✅ Store all user data in session
            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("PendingUserEmail", model.Email ?? "");
            HttpContext.Session.SetString("PendingUserPhone", model.PhoneNumber ?? "");
            HttpContext.Session.SetString("PendingUserFirstName", model.FirstName ?? "");
            HttpContext.Session.SetString("PendingUserLastName", model.LastName ?? "");
            HttpContext.Session.SetString("PendingUserRole", model.Role ?? "");
            HttpContext.Session.SetString("PendingUserPassword", model.Password ?? "");

            Console.WriteLine($"🔢 Generated Code: {verificationCode}");
            Console.WriteLine("✅ User details saved in session:");
            Console.WriteLine($"📌 Email: {model.Email}");
            Console.WriteLine($"📌 FirstName: {model.FirstName}");
            Console.WriteLine($"📌 LastName: {model.LastName}");
            Console.WriteLine($"📌 Role: {model.Role}");


            bool isSms = model.PreferredVerificationMethod == "sms";
            HttpContext.Session.SetString("IsSmsVerification", isSms.ToString());
            Console.WriteLine("✅ User details saved in session.");
            Console.WriteLine($"📌 IsSmsVerification = {isSms}");

            // ✅ Send verification code (Email ou SMS)
            try
            {
                if (model.PreferredVerificationMethod == "email")
                {
                    if (string.IsNullOrEmpty(model.Email))
                    {
                        ModelState.AddModelError("", "Veuillez entrer un email valide.");
                        return View(model);
                    }
                    Console.WriteLine($"📧 Envoi du code par EMAIL à {model.Email}");
                    Console.WriteLine($"📧 SMTP Server: {_emailSender}");
                    Console.WriteLine($"📧 Code généré : {verificationCode}");
                    await _emailSender.SendVerificationCode(model.Email, verificationCode);
                }
                else if (model.PreferredVerificationMethod == "sms")
                {
                    if (string.IsNullOrEmpty(model.PhoneNumber))
                    {
                        ModelState.AddModelError("", "Veuillez entrer un numéro de téléphone valide.");
                        return View(model);
                    }
                    Console.WriteLine($"📱 Envoi du code par SMS à {model.PhoneNumber}");
                    Console.WriteLine($"📱 Twilio AccountSid: {_smsSender}");
                    Console.WriteLine($"📱 Code généré : {verificationCode}");
                    await _smsSender.SendSmsCode(model.PhoneNumber, verificationCode);
                }
                else
                {
                    Console.WriteLine("❌ ERROR: Preferred verification method is invalid.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: Failed to send verification code - {ex.Message}");
                ModelState.AddModelError("", "Erreur lors de l'envoi du code de vérification. Veuillez réessayer.");
                return View(model);
            }

            // 6️⃣ Redirection vers la page de vérification avec les infos nécessaires
            return RedirectToAction("VerifyCode", new
            {
                email = model.Email,
                phone = model.PhoneNumber,
                preferredVerificationMethod = model.PreferredVerificationMethod
            });

            
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Console.WriteLine($"🔎 Tentative de connexion pour : 1");

            // 📌 Recherche de l'utilisateur dans la DB
            var user = await _userManager.FindByEmailAsync(model.Email);
            

            if (user == null)
            {
                Console.WriteLine("❌ Utilisateur non trouvé.");
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View(model);
            }

            Console.WriteLine($"🔎 Tentative de connexion pour :2 {user.Email}");
            Console.WriteLine($"🔎 L'utilisateur est-il bloqué ? {await _userManager.IsLockedOutAsync(user)}");

            // 🔐 Vérifier les identifiants
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            Console.WriteLine($"🔎 Le mot de passe est-il correct ? {isPasswordValid}");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            Console.WriteLine($"🔎 Tentative de connexion pour : {user.Email}");
            Console.WriteLine($"🔍 Authentification réussie ? {result.Succeeded}");
            Console.WriteLine($"🔍 Nécessite 2FA ? {result.RequiresTwoFactor}");
            Console.WriteLine($"🔍 Est verrouillé ? {result.IsLockedOut}");
            Console.WriteLine($"🔍 Utilisateur authentifié ? {_signInManager.IsSignedIn(User)}");

            if (result.Succeeded)
            {
                Console.WriteLine($"✅ Connexion réussie pour : {user.Email}");

                var userRoles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                // Ajouter tous les rôles de l'utilisateur
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // 🔥 La session expire après 1h
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                Console.WriteLine($"✅ Cookie créé pour {user.Email} - Expiration : {authProperties.ExpiresUtc}");


                // ✅ Ajouter une vérification après connexion
                Console.WriteLine($"🔎 Après connexion - Utilisateur authentifié ? {User.Identity.IsAuthenticated}");
                Console.WriteLine($"🔎 Après connexion - Utilisateur connecté : {User.Identity.Name}");

                return Redirect("/");
            }
            else if (result.IsLockedOut)
            {
                Console.WriteLine("❌ Compte verrouillé !");
                return View("Lockout");
            }else if (result.RequiresTwoFactor)
            {
                Console.WriteLine("🔄 Nécessite une vérification 2FA !");
                return RedirectToAction("VerifyCode");
            }

            Console.WriteLine("❌ Échec de connexion : Email ou mot de passe incorrect.");
            ModelState.AddModelError("", "Email ou mot de passe incorrect.");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {

            Console.WriteLine("🛠 Step 1: Entered VerifyCode method.");

            // 1️⃣ Vérifier si le code de session est valide
            var storedCode = HttpContext.Session.GetString("VerificationCode");
            if (storedCode == null || model.Code != storedCode)
            {
                Console.WriteLine("❌ ERROR: Code incorrect ou expiré !");
                ModelState.AddModelError("", "Code invalide ou expiré.");
                return View(model);
            }

            Console.WriteLine("✅ Step 2: Code verification passed.");

            // 2️⃣ Récupérer les infos de l'utilisateur en session
            var email = HttpContext.Session.GetString("PendingUserEmail");
            var phone = HttpContext.Session.GetString("PendingUserPhone");
            var firstName = HttpContext.Session.GetString("PendingUserFirstName");
            var lastName = HttpContext.Session.GetString("PendingUserLastName");
            var role = HttpContext.Session.GetString("PendingUserRole");
            var password = HttpContext.Session.GetString("PendingUserPassword");

            var isSmsVerification = HttpContext.Session.GetString("IsSmsVerification");
            if (!string.IsNullOrEmpty(isSmsVerification))
            {
                model.IsSmsVerification = bool.Parse(isSmsVerification);  // Convert string "true"/"false" to boolean
            }


            Console.WriteLine("✅ Checking Session Data...");
            Console.WriteLine($"📌 Email = {email ?? "NULL"}");
            Console.WriteLine($"📌 FirstName = {firstName ?? "NULL"}");
            Console.WriteLine($"📌 LastName = {lastName ?? "NULL"}");
            Console.WriteLine($"📌 Role = {role ?? "NULL"}");
            Console.WriteLine($"📌 Password = {password ?? "NULL"}");
            Console.WriteLine($"✅ DEBUG: IsSmsVerification = {model.IsSmsVerification}");


            // 3- Vérification des valeurs obligatoires
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("❌ ERROR: Missing required fields.");
                ModelState.AddModelError("", "Une ou plusieurs informations obligatoires sont manquantes.");
                return View(model);
            }

            // 4- Vérifier si l'utilisateur existe déjà avant de créer un nouveau compte
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                Console.WriteLine("✅ L'utilisateur existe déjà, connexion automatique...");
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // 5- Créer l'utilisateur et l'enregistrer dans la base
            Console.WriteLine("🛠 Step 4: Attempting to create user...");
            var user = new User
            {
                UserName = email,
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            Console.WriteLine("✅ Step 5: User object created.");

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                Console.WriteLine("❌ Step 6: ERROR: User creation failed.");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"❌ {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            Console.WriteLine("✅ Step 7: User successfully created.");

            // 6️⃣ Ajouter l’utilisateur à son rôle
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);


            // ✅ Auto login after successful registration
            await _signInManager.SignInAsync(user, isPersistent: false);
            Console.WriteLine("✅ step 8: User signed in successfully.");

            /*if (role == "Driver")
            {
                TempData["RedirectToDriverRegistration"] = "true";
                return RedirectToAction("Profile", "Profile");
            }*/

            // Nettoyer la session après validation
            //HttpContext.Session.Clear();


            // 8- Rediriger selon le rôle
            Console.WriteLine("🚀 step 9: Redirecting based on role...");
            if (role == "Driver")
            {
                // Toujours ajouter aussi le rôle "Passenger"
                await _userManager.AddToRoleAsync(user, "Passenger");
                Console.WriteLine("➡️ Redirection vers RegisterDriver...");
                return RedirectToAction("RegisterDriver", "DriverApplication");
            }
            else
            {
                Console.WriteLine("➡️ Redirection vers Home...");
                return RedirectToAction("Index", "Home");
            }


        }


        //Connexion avec reseaux sociaux
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            if (!User.Identity.IsAuthenticated)
            {
                var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

                // 🔥 Forcer la saisie des identifiants Google en supprimant la connexion persistante
                properties.Items["prompt"] = "consent";

                return Challenge(properties, provider);
            }

            return RedirectToAction("Index", "Home"); // ✅ Si l'utilisateur est déjà connecté, éviter de le renvoyer vers Google
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Erreur lors de la connexion externe : {remoteError}");
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // 🔎 Vérifier si l'utilisateur existe déjà dans la base de données avec son email
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // 🔍 Vérifier si l'utilisateur existe déjà
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                Console.WriteLine($"✅ L'utilisateur {email} existe déjà. Connexion en cours...");

                // 🔥 Vérifie si le compte Google est déjà lié, sinon on le fait
                var hasGoogleLogin = await _userManager.GetLoginsAsync(existingUser);
                if (!hasGoogleLogin.Any(l => l.LoginProvider == info.LoginProvider))
                {
                    await _userManager.AddLoginAsync(existingUser, info);
                }

                // 🔥 Vérifier si le rôle est défini
                if (string.IsNullOrEmpty(existingUser.Role))
                {
                    existingUser.Role = "Passenger"; // Assigner "Passenger" par défaut ou demander via RegisterExternal
                    await _userManager.UpdateAsync(existingUser);

                    /*Console.WriteLine($"⚠️ L'utilisateur {email} n'a pas encore de rôle. Redirection vers RegisterExternal...");
                    return RedirectToAction("RegisterExternal", new { email });*/
                }

                await _signInManager.SignInAsync(existingUser, isPersistent: true);

                return RedirectToAction("Index", "Home");
            }

            // 🔴 L'utilisateur n'existe pas encore, on le redirige vers l'inscription
            Console.WriteLine($"⚠️ L'utilisateur avec email {email} n'existe pas. Redirection vers l'inscription.");
            return RedirectToAction("RegisterExternal", new { email });
        }

        [HttpGet]
        public async Task<IActionResult> RegisterExternal(string returnUrl = null)
        {

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var phoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone) ?? "";

            var model = new RegisterExternalViewModel
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Role = "",
                IsExternalLogin = true, // ✅ On sait que c'est un login externe
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterExternal(RegisterExternalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔎 Vérifier si un utilisateur avec cet email existe déjà
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Un compte avec cet email existe déjà. Essayez de vous connecter.");
                return View(model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,  // 🔥 Le rôle choisi par l'utilisateur
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            Console.WriteLine($"✅ Utilisateur enregistré avec succès : {user.Email}, ID: {user.Id}");

            // 📌 Ajouter l’utilisateur à son rôle
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }
            await _userManager.AddToRoleAsync(user, model.Role);

            // 🔗 Associer l'authentification externe
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info != null)
            {
                await _userManager.AddLoginAsync(user, info);
            }
            else
            {
                ModelState.AddModelError("", "Erreur lors de la récupération des informations de connexion.");
                return View(model);
            }

            // ✅ Connexion automatique après l'inscription
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");

        }





    }



}

