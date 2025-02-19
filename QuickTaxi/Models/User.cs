using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "⚠️")]
        [EmailAddress(ErrorMessage = "L'email n'est pas au bon format.")]
        public override string Email { get; set; }

        [Required]
        [MaxLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; } // Enum: "Passenger", "Driver", "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Reward> Rewards { get; set; } = new List<Reward>();

        public Setting Settings { get; set; }

        // 2FA Fields
        public string? VerificationCode { get; set; }   // Store the code
        public DateTime? CodeExpiration { get; set; }  // Expiration time
    }
}
