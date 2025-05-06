using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class AffiliateCommission
{
    public int Id { get; set; }

    public int AffiliateId { get; set; }

    public int ProviderId { get; set; }

    public decimal AmountEarned { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Affiliate Affiliate { get; set; } = null!;

    public virtual Provider Provider { get; set; } = null!;
}
