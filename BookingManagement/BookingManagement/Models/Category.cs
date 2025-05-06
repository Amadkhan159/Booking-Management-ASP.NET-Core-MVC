using System;
using System.Collections.Generic;

namespace BookingManagement.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[]? Picture { get; set; }

    public virtual ICollection<Provider> Providers { get; set; } = new List<Provider>();
}
