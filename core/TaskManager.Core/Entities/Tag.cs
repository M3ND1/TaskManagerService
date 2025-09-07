using System;
namespace TaskManager.Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<ManagedTask>? ManagedTasks { get; set; }
    }
}