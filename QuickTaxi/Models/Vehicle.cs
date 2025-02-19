using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Vehicle
    {
        [Key]
        public Guid VehicleId { get; set; }

        [ForeignKey("Driver")]
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; }

        [Required]
        [MaxLength(100)]
        public string Model { get; set; }

        [Required]
        [MaxLength(50)]
        public string Color { get; set; }

        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; }
    }
}
