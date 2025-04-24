using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Data;
using QuickTaxi.Models;
using QuickTaxi.Helpers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stripe.Checkout;
using Stripe;
using CheckoutSessionOptions = Stripe.Checkout.SessionCreateOptions;
using CheckoutSession = Stripe.Checkout.Session;
using CheckoutSessionService = Stripe.Checkout.SessionService;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using QuickTaxi.helpers;


namespace QuickTaxi.Controllers
{
    [Authorize(Roles ="Passenger")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ReceiptGenerator _receiptGenerator;
       
        public PaymentController(ApplicationDbContext context, IConfiguration configuration, ReceiptGenerator receiptGenerator)
        {
            _context = context;
            _configuration = configuration;
            _receiptGenerator = receiptGenerator;
        }


        //Afficher la vue du paiement selon le type choisi
        [HttpGet]
        public async Task<IActionResult> Pay(Guid rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Passenger)
                .FirstOrDefaultAsync(r => r.RideId == rideId);
            if (ride == null || ride.PaymentStatus == "Paid")
            {
                TempData["ErrorMessage"] = "Course invalide ou déjà payée.";
                return RedirectToAction("MyRides", "Ride");
            } 

            var total = ride.EstimatedPrice;
            var tax = 0m;
            if (ride.PaymentMethod.ToLower() != "cash")
            {
                tax = total * 0.15m; //15% tax
                total += tax;
            }

            ViewBag.Tax = tax;
            ViewBag.Total = total;
            ViewBag.PublishableKey = _configuration["Stripe: PublishableKey"];
            return View(ride);
        }

        //checkout avec stripe: create session
        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(Guid rideId)
        {
            var ride = await _context.Rides
                .FirstOrDefaultAsync(r => r.RideId == rideId);
            if (ride == null || ride.PaymentStatus == "Paid")
            {
                return BadRequest();
            }

            var total = ride.EstimatedPrice;
            if(ride.PaymentMethod.ToLower() != "cash")
            {
                total += total * 0.15m;
            }

            var domain = "https://localhost:44364";

            var options = new CheckoutSessionOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "cad",
                            UnitAmount = (long)(total * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Course QuickTaxi - {ride.PickupAddress} → {ride.DestinationAddress}"
                            },
                        },
                        Quantity = 1,
                    }
                 },
                Mode = "payment",
                SuccessUrl = domain + $"/Payment/Success?rideId={ride.RideId}",
                CancelUrl = domain + "/Ride/MyRides"
            };
            var service = new CheckoutSessionService();
            CheckoutSession session = service.Create(options);
            return Redirect(session.Url);

        }

        //Succes de paiement avec stripe
        public async Task <IActionResult> Success(Guid rideId)
        {
            var ride = await _context.Rides
                .FirstOrDefaultAsync(r => r.RideId == rideId);
            if (ride != null)
            {
                ride.PaymentStatus = "Paid";
                //ride.Status = "Completed";
                _context.Rides.Update(ride);
                await _context.SaveChangesAsync();
            }

            _context.Payments.Add(new Payment
            {
                PaymentId = Guid.NewGuid(),
                RideId = ride.RideId,
                Amount = ride.EstimatedPrice,
                Method = ride.PaymentMethod,
                IsPaid = true,
                PaymentDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Paiement effectué avec succès.";
            return RedirectToAction("MyRides", "Ride");
        }

        //Marquer paiement en cash comme recu
        [Authorize(Roles ="Driver")]
        [HttpPost]
        public async Task<IActionResult> ConfirmCash(Guid rideId)
        {
            var ride = await _context.Rides
                .FirstOrDefaultAsync(r => r.RideId == rideId);
            if(ride == null || ride.PaymentMethod.ToLower() != "cash")
            {
                TempData["ErrorMessage"] = "Course introuvable ou méthode invalide.";
                return RedirectToAction("DriverRides", "Ride");
            }

            ride.PaymentStatus = "Paid";
            ride.Status = "Completed";
            _context.Rides.Update(ride);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Paiement reçu et course marquée comme terminée.";
            return RedirectToAction("DriverRides", "Ride");
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

