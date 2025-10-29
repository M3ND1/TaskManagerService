using TaskManager.Core.Enums;

namespace TaskManager.Application.DTOs
{
    public class ManagedTaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public PriorityLevel Priority { get; set; }
        public bool? IsCompleted { get; set; }
        public int? EstimatedHours { get; set; }
        public int? ActualHours { get; set; }
        public UserResponseDto? CreatedBy { get; set; }
        public UserResponseDto? AssignedTo { get; set; }
    }
}
