using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Reward
    {
        [Key]
        [Column("reward_id")]
        public Guid Id { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("Ride")]
        [Column("ride_id")]
        public Guid RideId { get; set; }
        public Ride Ride { get; set; }

        [Column("points_earned")]
        public int Points { get; set; } = 0;

        [Column("total_points")]
        public int PointsCount { get; set; }

        [Column("created_at")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
