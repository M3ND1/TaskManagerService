using TaskManager.Core.Enums;

namespace TaskManager.Core.Entities
{
    public class ManagedTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public bool IsCompleted { get; set; } = false;
        public int? EstimatedHours { get; set; }
        public int? ActualHours { get; set; }

        //FK
        public int? AssignedToId { get; set; }
        public int CreatedById { get; set; }

        public virtual User? AssignedTo { get; set; }
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<Tag>? Tags { get; set; }
    }
}