using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickTaxi.Models
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public string Language { get; set; } = "fr";

        public string PreferredVehicleType { get; set; } = "economique"; // "economique", "premium", "van"

        public bool Notifications { get; set; } = true;

        public virtual User User { get; set; }
    }
}
