using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace QuickTaxi.Helpers
{
    public class EmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailSender(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            _senderEmail = configuration["EmailSettings:SenderEmail"];
            _senderPassword = configuration["EmailSettings:SenderPassword"];
        }

        public async Task SendVerificationCode(string email, string code)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                client.EnableSsl = true; // ✅ Active la connexion sécurisée

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, "QuickTaxi"),
                    Subject = "Votre code de vérification QuickTaxi",
                    Body = $"Votre code est : {code}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"📧 Email envoyé à {email}");
            }
        }
    }
}
