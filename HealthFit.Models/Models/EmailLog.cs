using System;
using System.Collections.Generic;

namespace HealthFit.Models;

public partial class EmailLog
{
    public int EmailLogId { get; set; }

    public int? UserId { get; set; }

    public string? EmailType { get; set; }

    public DateTime? SentDate { get; set; }

    public string? Status { get; set; }

    public virtual User? User { get; set; }
}
