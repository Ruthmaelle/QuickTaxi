using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace QuickTaxi.Models
{
    public class Driver
    {
        [Key]
        [Column("driver_id")]
        public Guid DriverId { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Column("note_moyenne")]
        public decimal AverageRating { get; set; } = 0;


        // Personal Information
        [Required]
        [Column("date_of_birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [Column("license_number")]
        [StringLength(20)]
        public string LicenseNumber { get; set; }

        [Required]
        [Column("license_expiration")]
        [DataType(DataType.Date)]
        public DateTime? LicenseExpiration { get; set; }

        [Required]
        [Column("address")]
        public string Address { get; set; }

        [Column("profile_picture_url")]
        public string ProfilePictureUrl { get; set; }

        // Document Verification
        [Required]
        [Column("vehicle_registration_number")]
        [StringLength(50)]
        public string VehicleRegistrationNumber { get; set; }

        [Column("insurance_document_url")]
        public string InsuranceDocumentUrl { get; set; }

        [Column("license_document_url")]
        public string LicenseDocumentUrl { get; set; }

        // Approval Status
        [Column("is_approved")]
        public bool? IsApproved { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("statut")]
        public string Status { get; set; } = "Offline"; // Enum: "Available", "Busy", "Offline"


        //Relationship with Vehicles (One-to-One)
        public virtual Vehicle Vehicle { get; set; }


        [NotMapped]
        public IFormFile ProfilePicture { get; set; }

        [NotMapped]
        public IFormFile LicenseDocument { get; set; }

        [NotMapped]
        public IFormFile InsuranceDocument { get; set; }
    }
}
