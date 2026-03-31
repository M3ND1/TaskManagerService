namespace TaskManager.Application.DTOs.RefreshTokenDto;

public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
