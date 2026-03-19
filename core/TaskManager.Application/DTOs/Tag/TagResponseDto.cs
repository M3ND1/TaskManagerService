using TaskManager.Core.Entities;

namespace TaskManager.Application.DTOs.Tag;

public class TagResponseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Description { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedById { get; set; }
    public virtual User? CreatedBy { get; set; }
}
