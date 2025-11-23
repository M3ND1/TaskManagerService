namespace TaskManager.Application.DTOs;

public sealed record UserLoginResponseDto(string AccessToken, string RefreshToken);
