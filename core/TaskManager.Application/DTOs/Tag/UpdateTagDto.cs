using TaskManager.Core.Enums;

namespace TaskManager.Application.DTOs.Tag;
public class UpdateTagDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    // public PriorityLevel? Priority { get; set; }
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}
