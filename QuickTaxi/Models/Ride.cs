namespace QuickTaxi.Models
{
    public class Ride
    {
        public int Id { get; set; }
        public int PassengerId { get; set; }
        public int? DriverId { get; set; }
        public string PickupLocation { get; set; }
        public string DropoffLocation { get; set; }
        public string RideStatus { get; set; } // "pending", "accepted", "completed", "cancelled"
        public decimal? EstimatedFare { get; set; }
        public decimal? ActualFare { get; set; }
        public DateTime RideDate { get; set; } = DateTime.UtcNow;

        public User Passenger { get; set; }
        public Driver? Driver { get; set; }
    }
}
