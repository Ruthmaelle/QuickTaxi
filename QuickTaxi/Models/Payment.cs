using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        [ForeignKey("Ride")]
        public Guid RideId { get; set; }
        public Ride Ride { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public string Status { get; set; } // Enum: "Pending", "Completed", "Failed"

        [Required]
        public string Method { get; set; } // Enum: "Card", "Paypal", "Cash"

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}

