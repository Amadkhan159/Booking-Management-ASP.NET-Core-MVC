using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookingManagement.Models;

public partial class Availability
{
    public int Id { get; set; }

    public int ProviderId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly Date { get; set; }

    public int AvailableSlots { get; set; }

    public TimeOnly? OpenTime { get; set; } // 🕒 Start Time
    public TimeOnly? CloseTime { get; set; } // 🕰️ End Time
    public bool? IsClosed { get; set; } // 🚫 Closed for the day
    public bool? IsBlocked { get; set; } // 🔒 Special blocked date (e.g., holidays)
    public int UserId { get; set; }

    public virtual Provider Provider { get; set; } = null!;
}
