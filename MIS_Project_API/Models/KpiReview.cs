using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class KpiReview
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int CycleId { get; set; }

    public double? KpiTimeliness { get; set; }

    public double? KpiEfficiency { get; set; }

    public double? KpiCapacity { get; set; }

    public double? KpiManagerEvaluation { get; set; }

    public double? FinalKpiScore { get; set; }

    public bool? IsOverloaded { get; set; }

    public string? Note { get; set; }

    public virtual SystemConfig Cycle { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
