using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Payment
    {
        [Key]
        //public int Id { get; set; }
        [Column("id_paiement")]
        public Guid PaymentId { get; set; }

        [Column("id_reservation")]
        [ForeignKey("Ride")]
        public Guid RideId { get; set; }
        public Ride Ride { get; set; }

        //public string UserId { get; set; }

        [Column("montant")]
        public decimal Amount { get; set; }

        [Column("is_paid")]
        public bool IsPaid { get; set; }  = false;

        [Required]
        [Column("methode")]
        public string Method { get; set; } // Enum: "Card", "Paypal", "Cash"

        [Column("date_paiement")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    }
}

