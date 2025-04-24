using QuickTaxi.Models;
using System;
using System.Collections.Generic;

namespace QuickTaxi.ViewModels
{
    public class DriverDashboardViewModel
    {
        public int TotalRides { get; set; }
        public double TotalEarnings { get; set; }
        public double AverageRating { get; set; }
        public List<RideSummaryViewModel> UpcomingRides { get; set; }
        public List<ReviewSummaryViewModel> Reviews { get; set; } = new();

        public Driver Driver {  get; set; }
    }

    public class RideSummaryViewModel
    {
        public DateTime Date { get; set; }
        public string PassengerName { get; set; }
        public string StartAddress { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
    }

    public class ReviewSummaryViewModel
    {
        public double Rating { get; set; }
        public string Comment { get; set; }
        public string PassengerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserWithRolesViewModel
    {
        public string Id { get; set; } // 👈 Ajoute cette ligne
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public int TotalRides { get; set; }

        public bool IsSuspended { get; set; }
    }


}
