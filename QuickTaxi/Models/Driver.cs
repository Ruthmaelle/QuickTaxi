using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseNumber { get; set; }

        public bool IsAvailable { get; set; } = true;

        public virtual User User { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }
}
