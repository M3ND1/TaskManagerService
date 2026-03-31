using MediatR;
using TaskManager.Application.DTOs.RefreshTokenDto;

namespace TaskManager.Application.Features.Token.Commands;

public record RefreshTokenCommand(RefreshTokenRequest RefreshTokenRequest) : IRequest<RefreshTokenResponse>;