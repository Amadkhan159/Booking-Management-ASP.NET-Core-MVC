using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProviderId { get; set; }

    public string? Review1 { get; set; }

    public int? Rating { get; set; }

    public virtual User User { get; set; } = null!;
}
