using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using QuickTaxi.Models;
using QuickTaxi.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QuickTaxi.Helpers;
using System.Drawing;
using Twilio;
using QuickTaxi.helpers;

namespace QuickTaxi.Controllers
{
    public class RideController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly SmsSender _smsSender;
        private readonly EmailSender _emailSender;

        public RideController(ApplicationDbContext context, UserManager<User> userManager, SmsSender smsSender, EmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _smsSender = smsSender;
            _emailSender = emailSender;
        }

        private async Task SendApprovalNotification(User user)
        {
            string message = $"Bonjour {user.FirstName},  VOUS AVEZ UNE NOUVELLE COURSE DISPONIBLE.";
            await _smsSender.SendSmsCode(user.PhoneNumber, message);
            //await _emailSender.SendVerificationCode(user.Email, message);
        }

        private async Task SendRejectionNotification(User user)
        {
            string message = $"Bonjour {user.FirstName}, VOTRE DEMANDE DE COURSE A ETE MALHEURESEMENT REFUSER.";
            await _smsSender.SendSmsCode(user.PhoneNumber, message);
        }

        //1-Afficher la page de reservation
        [HttpGet]
        public async Task<IActionResult> RequestRide(string pickup, string destination)
        {
            var model = new RideRequestViewModel
            {
                AvailableRates = await _context.Rates.ToListAsync(),
                PickupAddress = pickup,
                DestinationAddress = destination
            };

            return View(model);
        }

        //2- Enregistrer une nouvelle reservation
        [HttpPost]
        public async Task<IActionResult> RequestRide(RideRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Erreur ModelState - Champ : {key} - Message : {error.ErrorMessage}");
                    }
                }

                // Recharge les tarifications pour afficher à nouveau le formulaire
                model.AvailableRates = await _context.Rates.ToListAsync();
                TempData["ErrorMessage"] = "Veuillez remplir tous les champs.";
                return View(model);
            }

            if (model.ScheduledDate != null && model.ScheduledDate < DateTime.Now)
            {
                ModelState.AddModelError("ScheduledDate", "Vous ne pouvez pas choisir une date antérieure.");
                return View(model);
            }


            //recuperer le passager
            //var passenger = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var passenger = await _userManager.GetUserAsync(User);


            if (passenger == null)
            {
                TempData["ErrorMessage"] = "Utilisateur Introuvable";
                return RedirectToAction("RequestRide");
            }

            //verifier l'existence d'un tarif 
            var rate = await _context.Rates.FirstOrDefaultAsync(r => r.IdTarification == model.RateId);
            
            if (rate == null)
            {
                TempData["ErrorMessage"] = "Aucun tarif disponible";
                return View(model);
            }

            Console.WriteLine("ScheduledDate reçu depuis le formulaire : " + model.ScheduledDate);


            //enregistrer la course
            var newRide = new Ride
            {
                RideId = Guid.NewGuid(),
                PassengerId = passenger.Id,
                PickupAddress = model.PickupAddress,
                DestinationAddress = model.DestinationAddress,
                PickupLatitude = model.PickupLatitude,
                PickupLongitude = model.PickupLongitude,
                DestinationLatitude = model.DestinationLatitude,
                DestinationLongitude = model.DestinationLongitude,
                DistanceKm = model.DistanceKm,
                EstimatedPrice = model.DistanceKm * rate.Multiplier,
                RateId = rate.IdTarification,
                PaymentMethod = model.PaymentMethod,
                Status = "Pending", //en attente d'un chauffeur des le debut
                RideDate = DateTime.UtcNow,
                ScheduledDate = model.ScheduledDate, // Nullable

            };

            _context.Rides.Add(newRide);
            await _context.SaveChangesAsync();

            if (model.PaymentMethod.ToLower() != "cash")
            {
                return RedirectToAction("Pay", "Payment", new { rideId = newRide.RideId });
            }


            if (newRide.ScheduledDate != null)
            {
                TempData["SuccessMessage"] = $"Votre course a été planifiée pour le {newRide.ScheduledDate?.ToLocalTime():dd MMM yyyy à HH:mm}.";

                string message = $"Bonjour {passenger.FirstName}, votre course QuickTaxi est planifiée pour le {newRide.ScheduledDate?.ToLocalTime():dd MMM yyyy à HH:mm}. Merci !";
                await _smsSender.SendSmsCode(passenger.PhoneNumber, message);
                await _emailSender.SendVerificationCode(passenger.Email, message);
            }
            else
            {
                TempData["SuccessMessage"] = "Votre course a été demandée avec succès.";
            }

            return RedirectToAction("MyRides");
        }

        //afficher les courses du user
        [HttpGet]
        public async Task<IActionResult> MyRides(string status)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if(user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur Introuvable";
                return RedirectToAction("RequestRide");
            }

            var rides = await _context.Rides
                .Where(r => r.PassengerId == user.Id)
                .Include(r => r.Driver)
                .ThenInclude(d => d.User)
                .OrderByDescending(r => r.RideDate)
                .ToListAsync();

            var ridesQuery = _context.Rides
                .Where(r => r.PassengerId == user.Id)
                .Include(r => r.Driver)
                .ThenInclude(d => d.User)
                .OrderByDescending(r => r.RideDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                ridesQuery = ridesQuery.Where(r => r.Status == status);
            }

            var ride = await ridesQuery.ToListAsync();

            var reviewedRideIds = await _context.Reviews
                .Where(r => r.RideId != Guid.Empty)
                .Select(r => r.RideId)
                .ToListAsync();

            ViewBag.ReviewedRideIds = reviewedRideIds;

            return View(rides);
        }


        //liste de courses pour les chauffeurs
        [Authorize(Roles ="Driver")]
        public async Task<IActionResult> AvailableRides()
        {
            // ✅ Trouver les rides expirées et non assignées
            var expiredPendingRides = await _context.Rides
                .Where(r =>
                    (r.Status == "Pending" || r.Status == "Assigned") &&
                    r.ScheduledDate != null &&
                    r.ScheduledDate < DateTime.Now)
                .ToListAsync();


            foreach (var ride in expiredPendingRides)
            {
                ride.Status = "Cancelled";
            }

            if (expiredPendingRides.Any())
                await _context.SaveChangesAsync();

            var currentDriver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == User.Identity.Name);

            if (currentDriver == null)
            {
                TempData["ErrorMessage"] = "Chauffeur introuvable.";
                return RedirectToAction("Dashboard", "Driver");
            }


            var availableRides = await _context.Rides
                .Where(r => r.Status == "Pending" || (r.Status == "Assigned" && r.DriverId == currentDriver.DriverId))
                .Include(r => r.Passenger)
                .OrderBy(r => r.RideDate)
                .ToListAsync();

            return View(availableRides);
        }

        //pouvoir accepter une course (Driver)
        [Authorize(Roles = "Driver")]
        [HttpPost]
        public async Task <IActionResult> AcceptRide(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .FirstOrDefaultAsync(r => r.RideId == rideId);


            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == User.Identity.Name);
            
            if (ride == null || driver == null)
            {
                TempData["ErrorMessage"] = "Utilisateur Introuvable";
                return RedirectToAction("RequestRide");
            }

            //assigner le chauffeur a la course qu'il choisi
            ride.DriverId = driver.DriverId;
            ride.Status = "Assigned";
            _context.Rides.Update(ride);
            await _context.SaveChangesAsync();

            //notification au passager
            var passenger = ride.Passenger;
            if(passenger != null)
            {
                string message = $"Bonjour {passenger.FirstName}, un chauffeur a été assigné à votre course ! 🚕";
                await _smsSender.SendSmsCode(passenger.PhoneNumber, message);
                await _emailSender.SendVerificationCode(passenger.Email, message);
            }

            TempData["SuccessMessage"] = "Course acceptée avec succès !";
            return RedirectToAction("DriverRides");

        }


        //rejeter une course
        [Authorize(Roles = "Driver")]
        [HttpPost]
        public async Task<IActionResult> RejectRides(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == User.Identity.Name);

            if (ride == null || driver == null)
            {
                TempData["ErrorMessage"] = "Données invalides.";
                return RedirectToAction("AvailableRides");
            }

            //verifier si le ride etait assigner par l'admin specifiquement a ce chauffeur
            if(ride.DriverId == driver.DriverId && ride.Status == "Assigned")
            {
                //supprimer la course
                _context.Rides.Remove(ride);
                await _context.SaveChangesAsync();

                //message au passenger
                var passenger = ride.Passenger;
                if (passenger != null)
                {
                    string message = $"Bonjour {passenger.FirstName}, votre course a été annulée par le chauffeur. Veuillez soumettre une nouvelle demande sur QuickTaxi.";
                    await _smsSender.SendSmsCode(passenger.PhoneNumber, message);
                    //await _emailSender.SendVerificationCode(passenger.Email, "Course refusée", message);
                }

                TempData["InfoMessage"] = "Course refusée et supprimée avec succès.";
                return RedirectToAction("AvailableRides");
            }
            else
            {
                // Le ride redevient libre (ou à réassigner)
                ride.DriverId = null;
                ride.Status = "Pending"; //
                _context.Rides.Update(ride);
                await _context.SaveChangesAsync();

                // Notification au passager
                //message au passenger
                var passenger = ride.Passenger;
                if (passenger != null)
                {
                    string message = $"Bonjour {passenger.FirstName}, le chauffeur a décliné votre course. Nous allons en chercher un autre.";
                    await _smsSender.SendSmsCode(passenger.PhoneNumber, message);
                    //await _emailSender.SendVerificationCode(passenger.Email, "Course refusée", message);
                }


                TempData["WarningMessage"] = "Vous avez refusé la course.";
                return RedirectToAction("AvailableRides");
            }

        }

        // Afficher les courses assignées à un chauffeur connecté
        [Authorize(Roles = "Driver")]
        [HttpGet]
        public async Task<IActionResult> DriverRides()
        {
            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == User.Identity.Name);

            if (driver == null)
            {
                TempData["ErrorMessage"] = "Chauffeur introuvable.";
                return RedirectToAction("AvailableRides");
            }

            var rides = await _context.Rides
                .Include(r => r.Passenger)
                .Where(r => r.DriverId == driver.DriverId 
                                && r.Status != "Cancelled")
                .OrderByDescending(r => r.RideDate)
                .ToListAsync();

            

            return View(rides); // return the DriverRides.cshtml view
        }

        [Authorize(Roles = "Driver")]
        [HttpPost]
        public IActionResult StartTracking(Guid rideId)
        {
            TempData["StartTime"] = DateTime.Now.ToString("HH:mm:ss");
            return RedirectToAction("TrackRide", new { rideId = rideId });
        }

        [Authorize(Roles = "Driver")]
        [HttpGet]
        public async Task<IActionResult> TrackRide(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null)
            {
                TempData["ErrorMessage"] = "Course introuvable.";
                return RedirectToAction("DriverRides");
            }

            TempData["StartTime"] = DateTime.Now.ToString("HH:mm:ss");
            return View(ride); // va charger TrackRide.cshtml avec les données du modèle
        }


        [Authorize(Roles = "Driver")]
        [HttpPost]
        public async Task<IActionResult> StartRide(Guid rideId)
        {
            var ride = await _context.Rides.FindAsync(rideId);
            if (ride != null)
            {
                ride.Status = "EnRoute";
                _context.Rides.Update(ride);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course démarrée.";
            }
            return RedirectToAction("DriverRides");
        }

        [Authorize(Roles = "Driver")]
        [HttpPost]
        public async Task<IActionResult> MarkArrived(Guid rideId)
        {
            var ride = await _context.Rides.FindAsync(rideId);
            if (ride != null)
            {
                ride.Status = "Arrived";
                _context.Rides.Update(ride);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Vous êtes arrivé à destination.";
            }
            return RedirectToAction("DriverRides");
        }

        [Authorize(Roles = "Driver")]
        [HttpPost]
        public async Task<IActionResult> CompleteRide(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null || ride.Status != "Assigned")
            {
                TempData["ErrorMessage"] = "Course introuvable.";
                return RedirectToAction("DriverRides");
            }

            
            ride.Status = "Completed";
            _context.Rides.Update(ride);

            //recompense au passager
            var totalPoints = await _context.Rewards
                    .Where(r => r.UserId == ride.PassengerId)
                    .SumAsync(r => r.Points);

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                UserId = ride.PassengerId,
                RideId = ride.RideId,
                Points = 10, // Exemple : 10 points par course
                PointsCount = totalPoints + 10,
                LastUpdated = DateTime.UtcNow
            };

            _context.Rewards.Add(reward);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] = "🎉 La course est terminée ! Laissez un avis pour votre passager.";
            return RedirectToAction("DriverRides");
        }



        // ADMIN: pour voir toutes les demandes de course
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RideRequests()
        {
            var rides = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver)//.ThenInclude(d => d.User)
                .Where(r => r.Status == "pending")
                .OrderByDescending(r => r.RideDate)
                .ToListAsync();

            var availableDrivers = await _context.Drivers
            .Include(d => d.User)
            .Where(d => d.IsApproved == true)
            .ToListAsync();

            ViewBag.AvailableDrivers = availableDrivers;


            return View(rides);
        }

        //ADMIN: Peut affecter un chauffeur a une course
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AssignDriver (Guid rideId, Guid driverId)
        {
            var ride = await _context.Rides.FirstOrDefaultAsync(r => r.RideId == rideId);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

            //var driver = await _context.Drivers
                //.Include(d => d.User)
                //.FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (ride == null || driver == null)
            {
                TempData["ErrorMessage"] = "Course ou chauffeur introuvable.";
                return RedirectToAction("RideRequests");
            } 

            ride.DriverId = driver.DriverId;
            ride.Status = "Assigned";
            _context.Rides.Update(ride);
            await _context.SaveChangesAsync();

            //envoie d'un message de notification
            await SendApprovalNotification(driver.User);

            TempData["SuccessMessage"] = "Chauffeur affecté avec succès !";
            return RedirectToAction("RideRequests");
        }

        //ADMIN: Refuser une course
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RejectRide(Guid rideId)
        {
            var ride = await _context.Rides.FindAsync(rideId);

            if(ride == null)
            {
                TempData["ErrorMessage"] = "Course introuvable.";
                return RedirectToAction("RideRequests");
            }

            ride.Status = "Cancelled";
            _context.Rides.Update(ride);
            await _context.SaveChangesAsync();

            //message de notification au user 
            if (ride.Passenger?.UserName != null)
            {
                await SendRejectionNotification(ride.Passenger);
            }

            TempData["SuccessMessage"] = "La course a été refusée.";
            return RedirectToAction("RideRequests");
        }

        // RideController.cs
        [Authorize(Roles = "Passenger")]
        [HttpGet]
        public async Task<IActionResult> LeaveReview(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null || ride.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Course introuvable ou non terminée.";
                return RedirectToAction("MyRides");
            }

            var review = new Review { RideId = rideId };
            return View(review);
        }

        [Authorize(Roles = "Passenger")]
        [HttpPost]
        public async Task<IActionResult> LeaveReview(Review review)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Champ: {key} | Erreur: {error.ErrorMessage}");
                    }
                }

                TempData["ErrorMessage"] = "Merci de compléter votre évaluation.";
                return View(review);
            }

            if (_context.Reviews.Any(r => r.RideId == review.RideId))
            {
                TempData["ErrorMessage"] = "Vous avez déjà évalué cette course.";
                return RedirectToAction("MyRides");
            }


            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Merci pour votre avis ! 🌟";
            return RedirectToAction("MyRides");
        }


        [Authorize(Roles = "Passenger")]
        [HttpPost]
        public async Task<IActionResult> CancelRide(Guid rideId)
        {
            var ride = await _context.Rides
                .FirstOrDefaultAsync(r => r.RideId == rideId && r.Status == "Pending");

            if (ride == null)
            {
                TempData["ErrorMessage"] = "Course introuvable ou déjà assignée.";
                return RedirectToAction("MyRides");
            }

            //_context.Rides.Remove(ride);
            ride.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Votre course a été annulée avec succès.";
            return RedirectToAction("MyRides");
        }

        
        public async Task<IActionResult> DriverReviews(Guid driverId)
        {
            if (driverId == Guid.Empty)
            {
                return NotFound("ID du chauffeur manquant.");
            }

            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (driver == null)
            {
                return NotFound("Chauffeur non trouvé.");
            }

            var reviews = await _context.Reviews
                .Include(r => r.Ride)
                    .ThenInclude(ride => ride.Passenger)
                .Include(r => r.Ride.Driver)
                .Where(r => r.Ride.DriverId == driverId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var noteMoyenne = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var viewModel = new DriverReviewsViewModel
            {
                Driver = driver,
                Reviews = reviews,
                AverageRating = noteMoyenne,
            };

            return View(viewModel);

        }


        [HttpGet]
        public async Task<IActionResult> EditScheduledRide(Guid rideId)
        {
            var ride = await _context.Rides.FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null || ride.Status.ToLower() != "pending" || ride.ScheduledDate == null)
            {
                TempData["ErrorMessage"] = "La course ne peut pas être modifiée.";
                return RedirectToAction("MyRides");
            }
            ViewBag.AvailableRates = await _context.Rates.ToListAsync();
            return View(ride);
        }


        [HttpPost]
        public async Task<IActionResult> EditScheduledRide(Ride updatedRide)
        {
            var ride = await _context.Rides.FirstOrDefaultAsync(r => r.RideId == updatedRide.RideId);

            if (ride == null || ride.Status.ToLower() != "pending")
            {
                TempData["ErrorMessage"] = "Impossible de modifier cette course.";
                return RedirectToAction("MyRides");
            }

            if (updatedRide.ScheduledDate == null || updatedRide.ScheduledDate <= DateTime.Now)
            {
                ModelState.AddModelError("ScheduledDate", "La date doit être future.");
                return View(ride);
            }

            ride.ScheduledDate = updatedRide.ScheduledDate;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"La course a été replanifiée pour le {ride.ScheduledDate?.ToLocalTime():dd MMM yyyy à HH:mm}.";
            return RedirectToAction("MyRides");
        }

        public async Task<IActionResult> Receipt(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .Include(r => r.Driver).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null || ride.PaymentStatus != "Paid")
            {
                return NotFound("Reçu introuvable ou course non payée.");
            }
            var pdfBytes = ReceiptGenerator.GeneratePdf(ride);
            return File(pdfBytes, "application/pdf", $"Recu_Admin_QuickTaxi_{ride.RideId}.pdf");
        }

    }
}
