namespace QuickTaxi.Models
{
    public class RewardSummary
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public int TotalPoints { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
