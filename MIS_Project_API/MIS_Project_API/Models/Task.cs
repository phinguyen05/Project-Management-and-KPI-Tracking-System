using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int ProjectId { get; set; }

    public int? ParentTaskId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? AssigneeId { get; set; }

    public string? Status { get; set; }

    public double? EstimatedTime { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool? RiskFlag { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? Assignee { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<ExtensionRequest> ExtensionRequests { get; set; } = new List<ExtensionRequest>();

    public virtual ICollection<Task> InverseParentTask { get; set; } = new List<Task>();

    public virtual ICollection<LogTime> LogTimes { get; set; } = new List<LogTime>();

    public virtual Task? ParentTask { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<TaskDependency> TaskDependencyPredecessorTasks { get; set; } = new List<TaskDependency>();

    public virtual ICollection<TaskDependency> TaskDependencySuccessorTasks { get; set; } = new List<TaskDependency>();

    public virtual User? UpdatedByNavigation { get; set; }
}
