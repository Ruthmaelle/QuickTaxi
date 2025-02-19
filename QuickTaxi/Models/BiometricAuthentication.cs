using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.Models
{
    public class BiometricAuthentication
    {
        [Key]
        public Guid BiometricAuthId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string BiometricType { get; set; } // Enum: "Fingerprint", "Face Recognition"

        [Required]
        public string DataHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
