using TaskManager.Core.Enums;

namespace TaskManager.Application.DTOs
{
    public class UpdateManagedTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public PriorityLevel? Priority { get; set; }
        public bool? IsCompleted { get; set; }
        public int? EstimatedHours { get; set; }
        public int? ActualHours { get; set; }
    }
}
