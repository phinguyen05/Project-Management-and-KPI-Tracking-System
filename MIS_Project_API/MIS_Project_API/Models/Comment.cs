using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public string? AttachedFile { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
