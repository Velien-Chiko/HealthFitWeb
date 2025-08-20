using System;
using System.Collections.Generic;

namespace HealthFit.Models;

public partial class BlogComment
{
    public int CommentId { get; set; }

    public int? BlogId { get; set; }

    public int? UserId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public bool? IsReply { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual User? User { get; set; }
}
