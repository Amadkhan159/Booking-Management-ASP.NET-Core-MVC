using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class Affiliate
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ReferralCode { get; set; } = null!;

    public decimal? CommissionRate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Balance { get; set; }

    public string? PayPalEmail { get; set; }

    public decimal? UserCredit { get; set; }
    public virtual ICollection<AffiliateCommission> AffiliateCommissions { get; set; } = new List<AffiliateCommission>();

    public virtual User User { get; set; } = null!;
}
