using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickTaxi.Models
{
    public class Report
    {
        [Key]
        [Column("Id")]
        public Guid ReportId { get; set; }

        [Required]
        [Column("UserId")]
        public string UserId { get; set; }  // L'utilisateur qui a signalé (passager ou chauffeur)

        [Required]
        [StringLength(100)]
        [Column("Subject")]
        public string Subject { get; set; }  // Sujet ou type du signalement

        [Required]
        [StringLength(1000)]
        [Column("Message")]
        public string Message { get; set; }  // Détails du signalement

        [DataType(DataType.DateTime)]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        // Navigation (optionnel)
        public User? User { get; set; }

        public string? Category { get; set; } //Ex: "Litige", "Remboursement", "Securite"

        public string? AdminResponse { get; set; } //message de retour a l'admin

        public bool RefundProcessed { get; set; } = false;
    }
}
