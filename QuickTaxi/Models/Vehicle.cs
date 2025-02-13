namespace QuickTaxi.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; } // "economique", "premium", "van"

        public Driver Driver { get; set; }
    }
}
