namespace TaskManager.Core.Interfaces;

public interface IJwtTokenGenerator
{
    public string GenerateToken(int userId, string email, string role);
    public string GenerateRefreshToken();

}
