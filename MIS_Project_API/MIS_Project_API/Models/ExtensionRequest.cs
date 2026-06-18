using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class ExtensionRequest
{
    public int RequestId { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime RequestedDeadline { get; set; }

    public string? Status { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
