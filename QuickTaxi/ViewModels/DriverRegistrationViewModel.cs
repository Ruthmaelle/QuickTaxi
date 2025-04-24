using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.ViewModels
{
    public class DriverRegistrationViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "La date de naissance est requise.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(DriverRegistrationViewModel), nameof(ValidateBirthDate))]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Le numéro de permis est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le numéro de permis ne peut pas dépasser 20 caractères.")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "La date d'expiration du permis est requise.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(DriverRegistrationViewModel), nameof(ValidateLicenseExpiration))]
        public DateTime LicenseExpiration { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string VehicleRegistrationNumber { get; set; }

        [Required]
        public string VehicleMake { get; set; }

        [Required]
        public string VehicleModel { get; set; }

        [Required(ErrorMessage = "Veuillez entrer la couleur du véhicule.")]
        public string VehicleColor { get; set; }

        [Required]
        [Range(1990, 2025, ErrorMessage = "L'année du véhicule doit être comprise entre 1990 et l'année actuelle.")]
        public int VehicleYear { get; set; }

        
        public string PhoneNumber { get; set; }

        // ✅ File Uploads (Not Stored in DB)

        // ✅ Profile Picture - Only Images Allowed
        [Required(ErrorMessage = "Veuillez télécharger une photo de profil.")]
        //[FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Seuls les formats JPG, JPEG et PNG sont autorisés.")]
        public IFormFile ProfilePicture { get; set; } // Optional file upload


        // ✅ License Document - Only PDF/DOC/DOCX Allowed
        [Required(ErrorMessage = "Veuillez télécharger une copie du permis de conduire.")]
        //[FileExtensions(Extensions = "pdf,doc,docx", ErrorMessage = "Seuls les formats PDF, DOC et DOCX sont autorisés.")]
        public IFormFile LicenseDocument { get; set; }


        // ✅ Insurance Document - Only PDF/DOC/DOCX Allowed
        [Required(ErrorMessage = "Veuillez télécharger un document d'assurance.")]
        //[FileExtensions(Extensions = "pdf,doc,docx", ErrorMessage = "Seuls les formats PDF, DOC et DOCX sont autorisés.")]
        public IFormFile InsuranceDocument { get; set; } // Optional file upload
        

        public static ValidationResult ValidateBirthDate(DateTime date, ValidationContext context)
        {
            var today = DateTime.Today;
            int age = today.Year - date.Year;
            if (date > today.AddYears(-age)) age--; 

            if(age < 18 || age > 85)
            {
                return new ValidationResult("L'âge du chauffeur doit être entre 18 et 85 ans.");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateLicenseExpiration(DateTime date, ValidationContext context)
        {
            if (date < DateTime.Today)
            {
                return new ValidationResult("La date d'expiration du permis ne peut pas être dans le passé.");
            }

            return ValidationResult.Success;
        }
    }
}
