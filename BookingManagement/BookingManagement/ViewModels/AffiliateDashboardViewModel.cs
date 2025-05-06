namespace BookingManagement.ViewModels
{
    public class AffiliateDashboardViewModel
    {
        public string ReferralCode { get; set; }
        public decimal UserCredit { get; set; }
        public List<ReferredUserViewModel> ReferredUsers { get; set; } = new List<ReferredUserViewModel>();
        public List<PaymentHistoryViewModel> PaymentHistory { get; set; } = new();
    }

    public class PaymentHistoryViewModel
    {
        public string FullName { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? DatePaid { get; set; }
    }

    public class ReferredUserViewModel
    {
        public string FullName { get; set; }
        public string? Phone { get; set; }
    }
}
