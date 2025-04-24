using QuickTaxi.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [ReadOnly(true)]
        public string Email { get; init; } // 📌 READ-ONLY, non modifiable

        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [ReadOnly(true)]
        public List<string> Roles { get; set; } = new List<string>();

        // ✅ Ajout de la vérification de demande en attente
        public bool HasPendingDriverRequest { get; set; } = false;
        public bool IsAlreadyDriver { get; set; } = false;


        // ✅ Liste des chauffeurs en attente (uniquement visible pour les Admins)
        public List<Driver> PendingDrivers { get; set; } = new List<Driver>();

        public Vehicle? Vehicle { get; set; } // ✅ To display vehicle details in Profile.cshtml
    }
}
