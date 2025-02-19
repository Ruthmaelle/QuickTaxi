using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.ViewModels
{
    public class VerifyCodeViewModel
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public bool IsSmsVerification { get; set; } // Indique si c'est une vérification par SMS
        public string PreferredVerificationMethod { get; set; } // "email" ou "sms"
    }

}
