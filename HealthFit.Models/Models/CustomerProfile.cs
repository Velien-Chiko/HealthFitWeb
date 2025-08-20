using System;
using System.Collections.Generic;

namespace HealthFit.Models;

public partial class CustomerProfile
{
    public int ProfileId { get; set; }

    public int? UserId { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? Gender { get; set; }

    public int? Age { get; set; }

    public int? Bmi { get; set; }

    public virtual User? User { get; set; }
}
