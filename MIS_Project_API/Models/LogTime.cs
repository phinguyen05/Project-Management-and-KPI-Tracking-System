using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class LogTime
{
    public int LogId { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public double ActualHours { get; set; }

    public DateOnly LogDate { get; set; }

    public string? AttachedFile { get; set; }

    public string? Status { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
    public DateTime CreatedAt { get; internal set; }
    public string? Description { get; internal set; }
}
