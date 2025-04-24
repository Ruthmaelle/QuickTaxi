using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QuickTaxi.Models;
using System;
using System.Threading.Tasks;

namespace QuickTaxi.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ✅ Vérifier si le rôle Admin existe
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // ✅ Vérifier si l'utilisateur Admin existe
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
                    PhoneNumber = "4383683837",
                    EmailConfirmed = true, // Confirmer l'email
                    CreatedAt = DateTime.UtcNow,
                    Role = "Admin"
                };

                // ✅ Ajouter l'utilisateur avec un mot de passe sécurisé
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine("✅ Admin créé avec succès !");
                }
                else
                {
                    Console.WriteLine("❌ Erreur lors de la création de l'Admin !");
                }
            }

            // ✅ Ajouter les rôles manquants
            /*string[] roles = { "Passenger", "Driver", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }*/
        }


        public static async Task CreateOrUpdateAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "admin0example@gmail.com";
            string adminPassword = "Admin@123";

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                // Création de l'admin s'il n'existe pas
                admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Mal",
                    LastName = "D",
                    PhoneNumber = "4383683837",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    Role = "Admin"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else
            {
                // Mise à jour des infos existantes
                bool updateNeeded = false;

                if (admin.FirstName != "Mal") { admin.FirstName = "Mal"; updateNeeded = true; }
                if (admin.LastName != "D") { admin.LastName = "D"; updateNeeded = true; }
                if (admin.PhoneNumber != "4383683837") { admin.PhoneNumber = "4383683837"; updateNeeded = true; }

                if (updateNeeded)
                {
                    await userManager.UpdateAsync(admin);
                }

                // 🔹 Mettre à jour le mot de passe si nécessaire
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                await userManager.ResetPasswordAsync(admin, token, adminPassword);
            }
        }

    }
}