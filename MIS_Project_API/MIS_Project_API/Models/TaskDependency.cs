using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class TaskDependency
{
    public int DependencyId { get; set; }

    public int PredecessorTaskId { get; set; }

    public int SuccessorTaskId { get; set; }

    public string? DependencyType { get; set; }

    public virtual Task PredecessorTask { get; set; } = null!;

    public virtual Task SuccessorTask { get; set; } = null!;
}
