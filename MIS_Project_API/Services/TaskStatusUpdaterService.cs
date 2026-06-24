using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIS_Project_API.Models;
using System;
using System.Threading;

namespace MIS_Project_API.Services;

public sealed class TaskStatusUpdaterService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskStatusUpdaterService> _logger;

    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public TaskStatusUpdaterService(IServiceScopeFactory scopeFactory, ILogger<TaskStatusUpdaterService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoScanAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await System.Threading.Tasks.Task.Delay(_interval, stoppingToken);
            await DoScanAsync(stoppingToken);
        }
    }

    private async System.Threading.Tasks.Task DoScanAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<MisProjectManagementContext>();

        var now = DateTime.Now;

        var tasks = await _context.Tasks
            .Include(t => t.Assignee)
            .Where(t => t.Deadline.HasValue
                        && t.Deadline.Value < now
                        && t.Status != "Done"
                        && t.AssigneeId != null)
            .ToListAsync(stoppingToken);

        if (tasks.Count == 0)
            return;

        foreach (var taskEntity in tasks)
        {
            if (!string.Equals(taskEntity.Status, "Overdue", StringComparison.OrdinalIgnoreCase))
            {
                taskEntity.Status = "Overdue";
            }

            var assignee = taskEntity.Assignee;
            var userEmail = assignee?.Email ?? "(unknown)";

            var content = $"[Trễ hạn] Task '{taskEntity.Name}' đã trễ hạn! Vui lòng cập nhật ngay.";

            _context.Notifications.Add(new Notification
            {
                UserId = taskEntity.AssigneeId!.Value,
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            _logger.LogInformation($"[MOCK EMAIL] Gửi email tới {userEmail}: Task {taskEntity.Name} đã trễ hạn!");
        }

        await _context.SaveChangesAsync(stoppingToken);
    }
}



