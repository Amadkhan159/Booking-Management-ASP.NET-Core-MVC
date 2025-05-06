using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class Provider
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string Phone { get; set; } = null!;

    public int? MaxBookingsPerDay { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[]? Image { get; set; }
    public string? Price { get; set; }

    public virtual ICollection<AffiliateCommission> AffiliateCommissions { get; set; } = new List<AffiliateCommission>();

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
