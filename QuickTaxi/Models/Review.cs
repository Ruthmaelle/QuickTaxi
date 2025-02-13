namespace QuickTaxi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int RideId { get; set; }
        public int PassengerId { get; set; }
        public int DriverId { get; set; }
        public int Rating { get; set; } // Between 1 and 5
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public Ride Ride { get; set; }
        public User Passenger { get; set; }
        public Driver Driver { get; set; }
    }
}
