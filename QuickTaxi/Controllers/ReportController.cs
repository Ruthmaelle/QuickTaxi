using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickTaxi.Models;
using QuickTaxi.Data;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace QuickTaxi.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReportController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //get
        public IActionResult Create()
        {
            return View();  
        }

        //Post
        [HttpPost]
        public async Task<IActionResult> Create (string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Veuillez remplir tous les champs.";
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var report = new Report
            {
                ReportId = Guid.NewGuid(),
                UserId = user.Id,
                Subject = subject,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Votre message a été envoyé à l’administrateur.";
            return RedirectToAction("Index", "Home");
        }

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

    }
}
