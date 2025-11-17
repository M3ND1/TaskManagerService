namespace TaskManager.Core.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Invalidated { get; set; } = false;
    public int UserId { get; set; }
    public int? ReplacedByTokenId { get; set; }
    public virtual User User { get; set; } = null!;
}