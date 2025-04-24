using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;

namespace QuickTaxi.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DriverController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (driver == null)
            {
                TempData["ErrorMessage"] = "Aucun profil de chauffeur trouvé.";
                return RedirectToAction("Profile", "Profile");
            }

            var reviews = await _context.Reviews
                .Include(r => r.Ride)
                    .ThenInclude(r => r.Passenger)
                .Where(r => r.Ride.DriverId == driver.DriverId)
                .Select(r => new ReviewSummaryViewModel
                {
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    PassengerName = r.Ride.Passenger.FirstName + " " + r.Ride.Passenger.LastName
                })
                .ToListAsync();


            var rides = await _context.Rides
                .Where(r => r.DriverId == driver.DriverId)
                .Include(r => r.Reviews)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalRides = rides.Count;
            var totalEarnings = rides.Sum(r => (double?)r.EstimatedPrice) ?? 0;
            var avgRating = rides.SelectMany(r => r.Reviews).Any()
                ? Math.Round(rides.SelectMany(r => r.Reviews).Average(r => r.Rating), 1)
                : 0;

            var earningsToday = rides
                .Where(r => r.RideDate.Date == today)
                .Sum(r => (double?)r.EstimatedPrice) ?? 0;

            var earningsWeek = rides
                .Where(r => r.RideDate.Date >= weekStart)
                .Sum(r => (double?)r.EstimatedPrice) ?? 0;

            var earningsMonth = rides
                .Where(r => r.RideDate.Date >= monthStart)
                .Sum(r => (double?)r.EstimatedPrice) ?? 0;

            var upcomingRides = rides
                .Where(r => r.ScheduledDate != null && r.ScheduledDate > DateTime.UtcNow && r.Status != "Completed")
                .OrderBy(r => r.ScheduledDate)
                .Select(r => new RideSummaryViewModel
                {
                    Date = r.ScheduledDate.Value,
                    PassengerName = r.Passenger?.FirstName + " " + r.Passenger?.LastName,
                    StartAddress = r.PickupAddress,
                    Destination = r.DestinationAddress,
                    Status = r.Status
                })
                .ToList();

            var viewModel = new DriverDashboardViewModel
            {
                TotalRides = totalRides,
                TotalEarnings = totalEarnings,
                AverageRating = avgRating,
                //EarningsToday = earningsToday,
                //EarningsThisWeek = earningsWeek,
                //EarningsThisMonth = earningsMonth,
                UpcomingRides = upcomingRides,
                Reviews = reviews,
                Driver = driver,
            };

            return View(viewModel);
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

            if(driver.IsApproved == true)
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
            if(user == null)
            {
                TempData["ErrorMessage"] = "❌ Impossible de trouver l'utilisateur associé.";
                return RedirectToAction("DriverRequests");
            }

            //ajouter le role "DRIVER"
            if(!await _roleManager.RoleExistsAsync("Driver"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Driver"));
            }
            await _userManager.AddToRoleAsync(user, "Driver");
            //await _userManager.RemoveFromRoleAsync(user, "Passenger"); // 🚀 Optionnel : supprimer du rôle passager si nécessaire
            

            TempData["SuccessMessage"] = "Le chauffeur a été approuvé avec succès.";
            return RedirectToAction("DriverRequests");
        }

        



    }
}
