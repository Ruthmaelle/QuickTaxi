using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public int Points { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; }
    }
}
