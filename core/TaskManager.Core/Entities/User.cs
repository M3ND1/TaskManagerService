namespace TaskManager.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Role { get; set; } = "User";

    public virtual ICollection<ManagedTask>? AssignedTasks { get; set; }
    public virtual ICollection<ManagedTask>? CreatedTasks { get; set; }
    public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
}