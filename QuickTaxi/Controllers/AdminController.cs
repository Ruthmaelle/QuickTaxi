using iText.StyledXmlParser.Jsoup.Select;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using QuickTaxi.helpers;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;
using static iText.Layout.Borders.Border;

namespace QuickTaxi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> AllRides(string status)
        {
            /*var rides = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                    .ThenInclude(d => d.User)
                .OrderByDescending(r => r.RideDate)
                .ToListAsync();*/

            var ridesQuery = _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                    .ThenInclude(d => d.User)
                .OrderByDescending(r => r.RideDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                ridesQuery = ridesQuery.Where(r => r.Status == status);
            }

            var rides = await ridesQuery.ToListAsync();

            return View(rides);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                .Where(r => r.PaymentStatus == "Paid")
                .OrderByDescending(r => r.RideDate)
                .ToListAsync();

            return View(payments);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Receipt (Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null || ride.PaymentStatus != "Paid")
                return NotFound();

            var pdfBytes = ReceiptGenerator.GeneratePdf(ride);
            return File(pdfBytes, "application/pdf", $"Recu_Admin_QuickTaxi_{ride.RideId}.pdf");
        }

        //statistiques
        public async Task<IActionResult> Dashboard()
        {
            var completedRides = await _context.Rides
                .CountAsync(r => r.Status == "Completed");
            var avgPrice = await _context.Rides
                .Where(r => r.Status == "Completed")
                .AverageAsync(r => r.EstimatedPrice);
            var activeDrivers = await _context.Drivers
                .CountAsync(d => d.IsApproved == true);

            var rewards = await _context.Rewards
            .Include(r => r.User)
            .GroupBy(r => r.UserId)
            .Select(g => new RewardSummary
            {
                UserId = Guid.NewGuid(),
                FullName = g.First().User.FirstName + " " + g.First().User.LastName,
                TotalPoints = g.Sum(x => x.Points),
                LastUpdated = g.Max(x => x.LastUpdated)
            })
            .ToListAsync();

            ViewBag.CompletedRides = completedRides;
            ViewBag.AvgPrice = avgPrice;
            ViewBag.ActiveDrivers = activeDrivers;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRewards()
        {
            var rewards = await _context.Rewards
            .Include(r => r.User) // pour accéder au nom du passager
            .OrderByDescending(r => r.LastUpdated)
            .ToListAsync();
            
            return View(rewards);
        }


        [HttpPost]
        public async Task<IActionResult> AddPoints (string userId, int points)
        {
            if (string.IsNullOrEmpty(userId) || points == 0)
            {
                TempData["ErrorMessage"] = "Utilisateur ou points invalides.";
                return RedirectToAction("ManageRewards");
            }

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Points = points,
                LastUpdated = DateTime.UtcNow,
                PointsCount = await _context.Rewards
                    .Where(r => r.UserId == userId)
                    .SumAsync(r => r.Points) + points
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Points ajoutés avec succès.";
            return RedirectToAction("ManageRewards");
        }

        /*[HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateReward(Guid userId, int additionalPoints)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var latestReward = await _context.Rewards
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.LastUpdated)
                .FirstOrDefaultAsync();

            var totalPoints = (latestReward?.PointsCount ?? 0) + additionalPoints;

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RideId = null,
                Points = additionalPoints,
                PointsCount = totalPoints,
                LastUpdated = DateTime.UtcNow
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🎁 Points mis à jour avec succès.";
            return RedirectToAction("Dashboard");
        }
        */


        //liste des rapports : admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllReports()
        {
            var reports = await _context.Reports
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        //marquer les reports commes resolu
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsResolved(Guid id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report != null)
            {
                report.IsResolved = true;
                _context.Reports.Update(report);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AllReports");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers(string role, string search, string sortOrder)
        {
            var users = await _context.Users.ToListAsync();

            var userRoles = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            // Dictionnaire UserId -> Liste de rôles
            var userRolesDict = users.ToDictionary(
                u => u.Id,
                u => userRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => roles.FirstOrDefault(r => r.Id == ur.RoleId)?.Name)
                        .Where(r => !string.IsNullOrEmpty(r))
                        .ToList()
            );

            // Projection en ViewModel
            var viewModel = users
                .Select(u => new UserWithRolesViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CreatedAt = u.CreatedAt,
                    Roles = userRolesDict.ContainsKey(u.Id) ? userRolesDict[u.Id] : new (),
                    IsSuspended = u.IsSuspended,
                    TotalRides = _context.Rides.Count(r => r.PassengerId == u.Id || r.Driver.UserId == u.Id)
                })
                .ToList();

            //filtrage
            if (!string.IsNullOrEmpty(role))
            {
                viewModel = viewModel.Where(u => u.Roles.Contains(role)).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                viewModel = viewModel.Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                                                 u.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            //tri
            viewModel = sortOrder switch
            {
                "date_desc" => viewModel.OrderByDescending(u => u.CreatedAt).ToList(),
                "date_asc" => viewModel.OrderByDescending(u => u.CreatedAt).ToList(),
                _ => viewModel
            };

            return View(viewModel);
        }

        // Suspendre un utilisateur
        [HttpPost]
        public async Task<IActionResult> SuspendUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "Action impossible.";
                return RedirectToAction("ManageUsers");
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddYears(100); // bloqué longtemps
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Utilisateur suspendu.";
            return RedirectToAction("ManageUsers");
        }

        // Réactiver un utilisateur
        [HttpPost]
        public async Task<IActionResult> ReactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction("ManageUsers");
            }

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Utilisateur réactivé.";
            return RedirectToAction("ManageUsers");
        }

        // Supprimer un utilisateur
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "Action non autorisée.";
                return RedirectToAction("ManageUsers");
            }

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "Utilisateur supprimé.";
            return RedirectToAction("ManageUsers");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleSuspension(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction("ManageUsers");
            }

            // Inverser l'état
            user.IsSuspended = !user.IsSuspended;

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = user.IsSuspended ? "Utilisateur suspendu." : "Utilisateur réactivé.";
            return RedirectToAction("ManageUsers");
        }

    }
}
