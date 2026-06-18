using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class LeaveDay
{
    public int LeaveId { get; set; }

    public int UserId { get; set; }

    public DateOnly LeaveDate { get; set; }

    public string Reason { get; set; } = null!;

    public string? Status { get; set; }

    public virtual User User { get; set; } = null!;
}
