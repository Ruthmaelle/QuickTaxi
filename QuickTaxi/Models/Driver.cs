using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Driver
    {
        [Key]
        public Guid DriverId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        public decimal AverageRating { get; set; } = 0;

        [Required]
        public string Status { get; set; } // Enum: "Available", "Busy", "Offline"
    }
}
