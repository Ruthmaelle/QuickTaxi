using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.Models
{
    public class Rate
    {
        [Key]
        public Guid RateId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RateName { get; set; }

        [Required]
        public decimal Multiplier { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
