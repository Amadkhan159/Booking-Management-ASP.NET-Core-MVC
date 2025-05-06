namespace BookingManagement.Models
{
    public class ProviderWithReviewsViewModel
    {
        public Provider Provider { get; set; }
        public ReviewViewModel? LatestReview { get; set; } // Add this property
    }

    public class ReviewViewModel
    {
        public string UserName { get; set; }
        public string? ReviewText { get; set; }
        public int Rating { get; set; } = 0;
    }
}
