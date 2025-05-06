using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public string? Type { get; set; }

    public string? Status { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual User User { get; set; } = null!;
}
