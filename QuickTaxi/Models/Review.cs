using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }

        [ForeignKey("Ride")]
        public Guid RideId { get; set; }
        public Ride Ride { get; set; }

        [Required]
        [Range(1, 5)]
        public decimal Rating { get; set; }

        public string? Comment { get; set; }
    }
}
