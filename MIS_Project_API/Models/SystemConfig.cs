using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class SystemConfig
{
    public int CycleId { get; set; }

    public string MonthYear { get; set; } = null!;

    public int? StandardWorkingHours { get; set; }

    public int? Holidays { get; set; }

    public string? PenaltyFactor { get; set; }

    public virtual ICollection<KpiReview> KpiReviews { get; set; } = new List<KpiReview>();
}
