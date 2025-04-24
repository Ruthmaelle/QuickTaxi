using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string NotificationType { get; set; } // Enum: "Ride Confirmed", "Payment Successful", etc.

        [Required]
        public string Status { get; set; } // Enum: "Unread", "Read"

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
