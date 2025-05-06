namespace BookingManagement.Models
{
    public class ReservationViewModel
    {
        public int ProviderId { get; set; }
        public int ProviderUserId { get; set; }
        public int SenderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;


        // Convert DateOnly to DateTime for compatibility
        public List<string> AvailableDates { get; set; } = new List<string>();

        // Use Dictionary to map dates to their open/close times
        public Dictionary<string, (string OpenTime, string CloseTime)> TimeSlots { get; set; }
            = new Dictionary<string, (string OpenTime, string CloseTime)>();

        public DateTime BookingTime { get; set; } // User-selected time for booking
        public string? Notes { get; set; } // User notes (e.g., room details)
    }

}
