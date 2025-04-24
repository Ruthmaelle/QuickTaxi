using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace QuickTaxi.Models
{
    public class Ride
    {
        [Key]
        [Column("ride_id")]
        public Guid RideId { get; set; }

        [ForeignKey("User")]
        [Column("passenger_id")]
        public string? PassengerId { get; set; }

        public User Passenger { get; set; }

        [ForeignKey("Driver")]
        [Column("driver_id")]
        public Guid? DriverId { get; set; }
        public Driver? Driver { get; set; }


        [Column("pickup_address")]
        public string? PickupAddress { get; set; }


        [Column("dropoff_address")]
        public string? DestinationAddress { get; set; }

        [Required]
        [Column("pickup_latitude")]
        public decimal PickupLatitude { get; set; }

        [Required]
        [Column("pickup_longitude")]
        public decimal PickupLongitude { get; set; }

        [Required]
        [Column("dropoff_latitude")]
        public decimal DestinationLatitude { get; set; }

        [Required]
        [Column("dropoff_longitude")]
        public decimal DestinationLongitude { get; set; }

        //[Required]
        [Column("distance_km")]
        public decimal DistanceKm { get; set; }

        //[Required]
        [Column("estimated_price")]
        public decimal EstimatedPrice { get; set; }

        [ForeignKey("Rate")]
        [Column("tarif_applique")]
        public Guid? RateId { get; set; }  //other name TarifApplique

        public Rate? RateApplied { get; set; }

       
        [Column("payment_method")]
        public string? PaymentMethod { get; set; } // Enum: "Card", "Paypal", "Cash"

        [Required]
        [Column("statut")]
        public string Status { get; set; } // Enum: "Pending", "Assigned", "Completed", "Cancelled"


        [Column("created_at")]
        public DateTime RideDate { get; set; } = DateTime.UtcNow;

        [Column("payment_status")]
        public string? PaymentStatus { get; set; }


        public Payment Payment { get; set; }
        public Reward Reward { get;  set; }

        [Column("scheduled_date")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledDate { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }

    }
}
