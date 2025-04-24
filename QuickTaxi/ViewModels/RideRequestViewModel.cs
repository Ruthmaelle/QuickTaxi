using QuickTaxi.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickTaxi.ViewModels
{
    public class RideRequestViewModel
    {
        [Required(ErrorMessage = "L'adresse de départ est obligatoire.")]
        public string PickupAddress { get; set; }

        [Required(ErrorMessage = "L'adresse de destination est obligatoire.")]
        public string DestinationAddress { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un type de véhicule.")]
        public Guid RateId { get; set; } // Correspond au type de tarif sélectionné (éco, premium...)

        public decimal EstimatedDistanceKm { get; set; }
        public decimal EstimatedPrice { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une méthode de paiement.")]
        public string PaymentMethod { get; set; } // "Card", "Cash", etc.

        [Display(Name = "Date de planification")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledDate { get; set; } // Pour les courses planifiées

        // GPS (optionnel si on veut ajouter une API de géoloc)
        [Required]
        public decimal PickupLatitude { get; set; }
        [Required]
        public decimal PickupLongitude { get; set; }
        [Required]
        public decimal DestinationLatitude { get; set; }
        [Required]
        public decimal DestinationLongitude { get; set; }

        [Required(ErrorMessage = "La distance doit être calculée.")]
        public decimal DistanceKm { get; set; }

        public List<Rate> AvailableRates { get; set; } = new(); //= new List<Rate>();

    }
}
