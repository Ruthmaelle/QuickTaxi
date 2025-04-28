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

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; } // Enum: "Passenger", "Driver", "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Reward> Rewards { get; set; }

        public Setting Settings { get; set; }

        public override string UserName { get; set; }

        // 2FA Fields
        public string? VerificationCode { get; set; }   
        public DateTime? CodeExpiration { get; set; }

        public bool IsSuspended { get; set; } = false;


    }
}
