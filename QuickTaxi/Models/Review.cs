using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Review
    {
        [Key]
        [Column("id_evaluation")]
        public Guid ReviewId { get; set; }

        //[ForeignKey("Ride")]
        [Column("id_reservation")]
        public Guid RideId { get; set; }

        [ForeignKey("RideId")]
        public Ride? Ride { get; set; }

        [Required(ErrorMessage = "La note est obligatoire.")]
        [Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5.")]
        [Column("note")]
        public int Rating { get; set; }

        [Column("commentaire")]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
