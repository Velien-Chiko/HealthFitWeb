using System;
using System.Collections.Generic;

namespace HealthFit.Models;

public partial class ProductReview
{
    public int ReviewId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Response { get; set; }

    public string? SellerReply { get; set; }

    public DateTime? ReplyAt { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
