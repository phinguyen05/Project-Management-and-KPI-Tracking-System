using System.ComponentModel.DataAnnotations;

namespace MIS_Project_API.DTOs.Task
{
    public class UpdateTaskDateDto
    {
        [Required]
        public DateTime NewStartDate { get; set; }

        [Required]
        public DateTime NewDueDate { get; set; }
    }
}