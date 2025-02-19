using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]  // ✅ Add validation for PhoneNumber
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; }  // "Passenger" ou "Driver"

        [Required]
        public string PreferredVerificationMethod { get; set; } // "email" ou "sms"
    }
}
