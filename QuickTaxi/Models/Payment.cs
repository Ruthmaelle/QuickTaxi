namespace QuickTaxi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int RideId { get; set; }
        public int PassengerId { get; set; }
        public string PaymentMethod { get; set; } // "card", "paypal", "cash"
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } // "pending", "completed", "failed"
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public Ride Ride { get; set; }
        public User Passenger { get; set; }
    }
}
