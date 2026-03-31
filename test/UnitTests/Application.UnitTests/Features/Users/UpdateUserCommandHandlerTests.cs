using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands.UpdateUser;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly IMapper _mapper;

    public UpdateUserCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_UpdateUser_WhenUserExistsAndDataIsValid()
    {
        // Arrange
        var existingUser = new User { Id = 1, FirstName = "Old", LastName = "Name", Email = "old@example.com" };
        var dto = new UpdateUserDto { FirstName = "New", LastName = "Updated" };
        _userRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateUserCommand(1, dto), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_SetUpdatedAt_ToUtcNow()
    {
        // Arrange
        var existingUser = new User { Id = 1, FirstName = "John" };
        _userRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapper);

        // Act
        await handler.Handle(new UpdateUserCommand(1, new UpdateUserDto()), CancellationToken.None);

        // Assert
        existingUser.UpdatedAt.Should().NotBeNull();
        existingUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateUserCommand(99, new UpdateUserDto()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenUserIdIsNegative()
    {
        // Arrange
        var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateUserCommand(-1, new UpdateUserDto()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenUpdateFails()
    {
        // Arrange
        var existingUser = new User { Id = 1, FirstName = "John" };
        _userRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateUserCommand(1, new UpdateUserDto()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
