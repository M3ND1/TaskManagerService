namespace TaskManager.Application.DTOs;

public class RefreshTokenDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool Invalidated { get; set; }
}
