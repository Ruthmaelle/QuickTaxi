using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.ViewModels
{
    public class RegisterExternalViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } // Ajoute le numéro de téléphone

        [Required]
        public string Role { get; set; } // Permettre à l'utilisateur de choisir entre "Passenger" ou "Driver"

        public string ReturnUrl { get; set; }
        public bool IsExternalLogin { get; set; }
    }
}
