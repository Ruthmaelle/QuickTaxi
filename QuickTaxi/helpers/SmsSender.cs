using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;
using System.Threading.Tasks;

namespace QuickTaxi.Helpers
{
    public class SmsSender
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _twilioNumber;

        public SmsSender(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _twilioNumber = configuration["Twilio:PhoneNumber"];


            Console.WriteLine($"Twilio SID: {_accountSid}");
            Console.WriteLine($"Twilio Token: {_authToken}");
            Console.WriteLine($"Twilio Number: {_twilioNumber}");



            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_twilioNumber))
            {
                throw new Exception("Twilio credentials are missing. Check your appsettings.json or environment variables.");
            }
        }

        public async Task SendSmsCode(string recipientPhone, string verificationCode)
        {
            TwilioClient.Init(_accountSid, _authToken);

            var message = await MessageResource.CreateAsync(
                body: $"Votre code de vérification QuickTaxi est : {verificationCode}",
                from: new PhoneNumber(_twilioNumber),
                to: new PhoneNumber(recipientPhone)
            );

            Console.WriteLine($"📩 SMS envoyé avec SID : {message.Sid}");
        }
    }
}
