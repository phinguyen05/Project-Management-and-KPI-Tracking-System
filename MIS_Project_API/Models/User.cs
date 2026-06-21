using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? Role { get; set; }

    public int? DepartmentId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<ExtensionRequest> ExtensionRequestApprovedByNavigations { get; set; } = new List<ExtensionRequest>();

    public virtual ICollection<ExtensionRequest> ExtensionRequestUsers { get; set; } = new List<ExtensionRequest>();

    public virtual ICollection<KpiReview> KpiReviews { get; set; } = new List<KpiReview>();

    public virtual ICollection<LeaveDay> LeaveDays { get; set; } = new List<LeaveDay>();

    public virtual ICollection<LogTime> LogTimeApprovedByNavigations { get; set; } = new List<LogTime>();

    public virtual ICollection<LogTime> LogTimeUpdatedByNavigations { get; set; } = new List<LogTime>();

    public virtual ICollection<LogTime> LogTimeUsers { get; set; } = new List<LogTime>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Task> TaskAssignees { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskUpdatedByNavigations { get; set; } = new List<Task>();

    public virtual ICollection<Project> ProjectsNavigation { get; set; } = new List<Project>();
}
