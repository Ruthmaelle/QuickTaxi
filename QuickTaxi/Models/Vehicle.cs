using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Vehicle
    {
        [Key]
        [Column("vehicle_id")]
        public Guid VehicleId { get; set; }

        [ForeignKey("Driver")]
        [Column("driver_id")]
        public Guid DriverId { get; set; }

        public virtual Driver Driver { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("marque")]
        public string Brand { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("modele")]
        public string Model { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("couleur")]
        public string Color { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("year")]
        public int Year { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("license_plate")]
        public string LicensePlate { get; set; }

        // ✅ Document Verification
        [Required]
        [Column("vehicle_registration_number")]
        [StringLength(50)]
        public string VehicleRegistrationNumber { get; set; }

        [Column("insurance_document_url")]
        public string InsuranceDocumentUrl { get; set; }

        [Column("license_document_url")]
        public string LicenseDocumentUrl { get; set; }
    }
}
