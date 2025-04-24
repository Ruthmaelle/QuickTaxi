using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTaxi.Models
{
    public class Rate
    {
        [Key]
        [Column("id_tarification")]
        public Guid IdTarification { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("nom_tarification")]
        public string NomTarification { get; set; }

        [Required]
        [Column("multiplicateur")]
        public decimal Multiplier { get; set; }

        [Column("heure_debut")]
        public TimeSpan StartTime { get; set; }

        [Column("heure_fin")]
        public TimeSpan EndTime { get; set; }
    }
}
