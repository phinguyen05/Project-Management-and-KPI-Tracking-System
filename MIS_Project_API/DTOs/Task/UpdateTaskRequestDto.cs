using System;
using System.ComponentModel.DataAnnotations;

namespace MIS_Project_API.DTOs.Task;

public class UpdateTaskRequestDto
{
    [Required(ErrorMessage = "Tên task không được để trống")]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public int? AssigneeId { get; set; }

    [Required(ErrorMessage = "Trạng thái không được để trống")]
    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public double? EstimatedTime { get; set; }

    public DateTime? StartDate { get; set; }

    // dueDate (DueDate/Deadline) theo convention FE
    public DateTime? dueDate { get; set; }
}

