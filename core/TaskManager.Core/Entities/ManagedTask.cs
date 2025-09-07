using System;
using System.Collections.Generic;
using TaskManager.Core.Enums;
using TaskManager.Core.Entities;

namespace TaskManager.Core.Entities
{
    public class ManagedTask
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public PriorityLevel? Priority { get; set; }
        public bool IsCompleted { get; set; } = false;
        public IEnumerable<Tag>? Tags { get; set; }
    }
}