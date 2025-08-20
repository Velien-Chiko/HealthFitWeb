using System;
using System.Collections.Generic;

namespace HealthFit.Models.Models;

public partial class Aiinteraction
{
    public int InteractionId { get; set; }

    public int? UserId { get; set; }

    public string QueryText { get; set; } = null!;

    public string? ResponseText { get; set; }

    public DateTime? InteractionDate { get; set; }

    public virtual User? User { get; set; }
}
