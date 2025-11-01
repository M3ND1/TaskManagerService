using TaskManager.Core.Enums;

namespace TaskManager.Application.DTOs
{
    public class CreateManagedTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.Low;
        public int? EstimatedHours { get; set; }
    }
}
