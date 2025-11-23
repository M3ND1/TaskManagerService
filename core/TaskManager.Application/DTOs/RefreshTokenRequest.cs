namespace TaskManager.Application.DTOs;

public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
