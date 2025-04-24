using QuickTaxi.Models;

namespace QuickTaxi.ViewModels
{
    public class DriverReviewsViewModel
    {
        public Driver Driver { get; set; }
        public List<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
    }

}
