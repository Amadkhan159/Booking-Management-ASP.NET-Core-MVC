using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProviderId { get; set; }

    public DateTime BookingDate { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Nights { get; set; }
    public string? People { get; set; }

    public virtual Provider Provider { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
