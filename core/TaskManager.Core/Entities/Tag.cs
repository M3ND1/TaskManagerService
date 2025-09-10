namespace TaskManager.Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //FK
        public int? CreatedById { get; set; }
        public virtual User? CreatedBy { get; set; }

        public virtual ICollection<ManagedTask>? ManagedTasks { get; set; }
    }
}