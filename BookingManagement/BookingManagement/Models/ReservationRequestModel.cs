namespace BookingManagement.Models
{
    public class ReservationRequestModel
    {
        public int ProviderId { get; set; }
        public DateOnly SelectedDate { get; set; }
        public TimeOnly SelectedTime { get; set; }
        public string? Notes { get; set; }
        public string? Nights { get; set; }
        public string? People { get; set; }
    }
}
