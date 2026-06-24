using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MIS_Project_API.Models;

public partial class MisProjectManagementContext : DbContext
{
    public MisProjectManagementContext()
    {
    }

    public MisProjectManagementContext(DbContextOptions<MisProjectManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<ExtensionRequest> ExtensionRequests { get; set; }

    public virtual DbSet<KpiReview> KpiReviews { get; set; }

    public virtual DbSet<LeaveDay> LeaveDays { get; set; }

    public virtual DbSet<LogTime> LogTimes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectDocument> ProjectDocuments { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskDependency> TaskDependencies { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Audit_Lo__9E2397E0E2604318");

            entity.ToTable("Audit_Log");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.TargetTable)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("target_table");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Audit_Log__user___04E4BC85");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__E7957687954C5FBB");

            entity.ToTable("Comment");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.AttachedFile).HasColumnName("attached_file");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Task).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comment__task_id__74AE54BC");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comment__user_id__75A278F5");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C223242206C31B33");

            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Manager).WithMany(p => p.Departments)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Department_Manager");
        });

        modelBuilder.Entity<ExtensionRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Extensio__18D3B90F5EF86114");

            entity.ToTable("Extension_Request");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RequestedDeadline)
                .HasColumnType("datetime")
                .HasColumnName("requested_deadline");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ExtensionRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__Extension__appro__7B5B524B");

            entity.HasOne(d => d.Task).WithMany(p => p.ExtensionRequests)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Extension__task___797309D9");

            entity.HasOne(d => d.User).WithMany(p => p.ExtensionRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Extension__user___7A672E12");
        });

        modelBuilder.Entity<KpiReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__KPI_Revi__60883D90B03C76B6");

            entity.ToTable("KPI_Review");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.CycleId).HasColumnName("cycle_id");
            entity.Property(e => e.FinalKpiScore).HasColumnName("final_kpi_score");
            entity.Property(e => e.IsOverloaded)
                .HasDefaultValue(false)
                .HasColumnName("is_overloaded");
            entity.Property(e => e.KpiCapacity).HasColumnName("kpi_capacity");
            entity.Property(e => e.KpiEfficiency).HasColumnName("kpi_efficiency");
            entity.Property(e => e.KpiManagerEvaluation).HasColumnName("kpi_manager_evaluation");
            entity.Property(e => e.KpiTimeliness).HasColumnName("kpi_timeliness");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Cycle).WithMany(p => p.KpiReviews)
                .HasForeignKey(d => d.CycleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KPI_Revie__cycle__01142BA1");

            entity.HasOne(d => d.User).WithMany(p => p.KpiReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KPI_Revie__user___00200768");
        });

        modelBuilder.Entity<LeaveDay>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__Leave_Da__743350BC38855AE3");

            entity.ToTable("Leave_Days");

            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.LeaveDate).HasColumnName("leave_date");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.LeaveDays)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Leave_Day__user___0D7A0286");
        });

        modelBuilder.Entity<LogTime>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Log_Time__9E2397E0A2748333");

            entity.ToTable("Log_Time", tb => tb.HasTrigger("trg_AuditLog_LogTime"));

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.ActualHours).HasColumnName("actual_hours");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.AttachedFile).HasColumnName("attached_file");
            entity.Property(e => e.LogDate).HasColumnName("log_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LogTimeApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__Log_Time__approv__6FE99F9F");

            entity.HasOne(d => d.Task).WithMany(p => p.LogTimes)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Log_Time__task_i__6E01572D");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.LogTimeUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Log_Time__update__70DDC3D8");

            entity.HasOne(d => d.User).WithMany(p => p.LogTimeUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Log_Time__user_i__6EF57B66");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotifId).HasName("PK__Notifica__CDF18E6C0D7559D3");

            entity.ToTable("Notification");

            entity.Property(e => e.NotifId).HasColumnName("notif_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__user___09A971A2");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__BC799E1F5C25D377");

            entity.ToTable("Project");

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Manager).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Project__manager__5812160E");

            entity.HasMany(d => d.Clients).WithMany(p => p.ProjectsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectClient",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Project_C__clien__5BE2A6F2"),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Project_C__proje__5AEE82B9"),
                    j =>
                    {
                        j.HasKey("ProjectId", "ClientId").HasName("PK__Project___E78B845DCD8143C5");
                        j.ToTable("Project_Client");
                        j.IndexerProperty<int>("ProjectId").HasColumnName("project_id");
                        j.IndexerProperty<int>("ClientId").HasColumnName("client_id");
                    });
        });

        modelBuilder.Entity<ProjectDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Project___9666E8ACC2046EE6");

            entity.ToTable("Project_Document");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectDocuments)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Project_D__proje__5EBF139D");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.CycleId).HasName("PK__System_C__5D9558815DD4AAFE");

            entity.ToTable("System_Config");

            entity.Property(e => e.CycleId).HasColumnName("cycle_id");
            entity.Property(e => e.Holidays).HasColumnName("holidays");
            entity.Property(e => e.MonthYear)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("month_year");
            entity.Property(e => e.PenaltyFactor).HasColumnName("penalty_factor");
            entity.Property(e => e.StandardWorkingHours).HasColumnName("standard_working_hours");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Task__0492148DD5BCA89C");

            entity.ToTable("Task", tb => tb.HasTrigger("trg_AuditLog_Task"));

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.AssigneeId).HasColumnName("assignee_id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime")
                .HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstimatedTime).HasColumnName("estimated_time");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentTaskId).HasColumnName("parent_task_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.RiskFlag)
                .HasDefaultValue(false)
                .HasColumnName("risk_flag");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Assignee).WithMany(p => p.TaskAssignees)
                .HasForeignKey(d => d.AssigneeId)
                .HasConstraintName("FK__Task__assignee_i__6477ECF3");

            entity.HasOne(d => d.ParentTask).WithMany(p => p.InverseParentTask)
                .HasForeignKey(d => d.ParentTaskId)
                .HasConstraintName("FK__Task__parent_tas__656C112C");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__project_id__6383C8BA");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TaskUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Task__updated_by__66603565");

            // Normalize allowed Task statuses to avoid CHECK constraint violations
            // (DB constraint must also be updated via migration)
            entity.HasCheckConstraint("CK_Task_Status",
                "[status] IN ('To_Do','Doing','Done','Overdue')");
        });

        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.HasKey(e => e.DependencyId).HasName("PK__Task_Dep__952A1872F49C773A");

            entity.ToTable("Task_Dependency");

            entity.Property(e => e.DependencyId).HasColumnName("dependency_id");
            entity.Property(e => e.DependencyType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("dependency_type");
            entity.Property(e => e.PredecessorTaskId).HasColumnName("predecessor_task_id");
            entity.Property(e => e.SuccessorTaskId).HasColumnName("successor_task_id");

            entity.HasOne(d => d.PredecessorTask).WithMany(p => p.TaskDependencyPredecessorTasks)
                .HasForeignKey(d => d.PredecessorTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task_Depe__prede__693CA210");

            entity.HasOne(d => d.SuccessorTask).WithMany(p => p.TaskDependencySuccessorTasks)
                .HasForeignKey(d => d.SuccessorTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task_Depe__succe__6A30C649");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370FD71BE8C8");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E6164E897C1E7").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__User__F3DBC572915D1B50").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__User__department__534D60F1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
