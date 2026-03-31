using FluentAssertions;
using Moq;
using TaskManager.Application.Features.Users.Commands.DeleteUser;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Users;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    [Fact]
    public async Task Handle_Should_DeleteUser_WhenUserExists()
    {
        // Arrange
        var existingUser = new User { Id = 1, FirstName = "John", Email = "john@example.com" };
        _userRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteUserCommandHandler(_userRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _userRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = new DeleteUserCommandHandler(_userRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteUserCommand(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenDeleteFails()
    {
        // Arrange
        var existingUser = new User { Id = 1, FirstName = "John" };
        _userRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteUserCommandHandler(_userRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
