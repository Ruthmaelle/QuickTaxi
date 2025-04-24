using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickTaxi.Data;
using QuickTaxi.Helpers;
using QuickTaxi.Models;
using QuickTaxi.helpers;

public class RideReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RideReminderService> _logger;

    public RideReminderService(IServiceProvider serviceProvider, ILogger<RideReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("[RAPPEL] Vérification des courses planifiées...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var smsSender = scope.ServiceProvider.GetRequiredService<SmsSender>();
                var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

                var now = DateTime.UtcNow;
                var windowStart = now.AddMinutes(50);
                var windowEnd = now.AddMinutes(70);

                var upcomingRides = context.Rides
                    .Where(r => r.ScheduledDate != null
                        && r.Status.ToLower() == "pending"
                        && r.ScheduledDate >= windowStart
                        && r.ScheduledDate <= windowEnd)
                    .ToList();

                foreach (var ride in upcomingRides)
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == ride.PassengerId);
                    if (user == null) continue;

                    string reminderMessage = $"Rappel : votre course QuickTaxi est prévue pour {ride.ScheduledDate?.ToLocalTime():dd MMM yyyy à HH:mm}. Soyez prêt(e) !";

                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                        await smsSender.SendSmsCode(user.PhoneNumber, reminderMessage);

                    if (!string.IsNullOrEmpty(user.Email))
                        await emailSender.SendVerificationCode(user.Email, reminderMessage);

                    _logger.LogInformation($"[RAPPEL PASSAGER] Envoyé à {user.Email ?? user.PhoneNumber} pour la course {ride.RideId}");
                }

                // 🔔 Rappel aux chauffeurs pour les courses acceptées
                var acceptedRides = context.Rides
                    .Where(r => r.ScheduledDate != null
                        && r.DriverId != null
                        && r.Status.ToLower() == "assigned"
                        && r.ScheduledDate >= windowStart
                        && r.ScheduledDate <= windowEnd)
                    .ToList();

                foreach (var ride in acceptedRides)
                {
                    var driver = context.Drivers
                        .Where(d => d.DriverId == ride.DriverId)
                        .Select(d => d.User)
                        .FirstOrDefault();

                    if (driver == null) continue;

                    string message = $"Rappel : vous avez une course QuickTaxi planifiée pour {ride.ScheduledDate?.ToLocalTime():dd MMM yyyy à HH:mm}. Soyez à l'heure !";

                    if (!string.IsNullOrEmpty(driver.PhoneNumber))
                        await smsSender.SendSmsCode(driver.PhoneNumber, message);

                    if (!string.IsNullOrEmpty(driver.Email))
                        await emailSender.SendVerificationCode(driver.Email, message);

                    _logger.LogInformation($"[RAPPEL CHAUFFEUR] Envoyé à {driver.Email ?? driver.PhoneNumber} pour la course {ride.RideId}");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
