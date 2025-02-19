using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Ride
    {
        [Key]
        public Guid RideId { get; set; }

        [ForeignKey("User")]
        public string PassengerId { get; set; }
        public User Passenger { get; set; }

        [ForeignKey("Driver")]
        public Guid? DriverId { get; set; }
        public Driver? Driver { get; set; }

        [Required]
        public string PickupAddress { get; set; }

        [Required]
        public string DestinationAddress { get; set; }

        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }

        public decimal DestinationLatitude { get; set; }
        public decimal DestinationLongitude { get; set; }

        public decimal DistanceKm { get; set; }
        public decimal EstimatedPrice { get; set; }

        [ForeignKey("Rate")]
        public Guid RateId { get; set; }
        public Rate RateApplied { get; set; }

        [Required]
        public string PaymentMethod { get; set; } // Enum: "Card", "Paypal", "Cash"

        [Required]
        public string Status { get; set; } // Enum: "Pending", "Assigned", "Completed", "Cancelled"

        public DateTime RideDate { get; set; } = DateTime.UtcNow;

        public Payment Payment { get; set; }
        public Reward Reward { get;  set; }
    }
}
