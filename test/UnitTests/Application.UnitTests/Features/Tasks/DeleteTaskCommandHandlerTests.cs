using FluentAssertions;
using Moq;
using TaskManager.Application.Features.Tasks.Commands.DeleteTask;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tasks;

public class DeleteTaskCommandHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();

    [Fact]
    public async Task Handle_Should_DeleteTask_WhenTaskExists()
    {
        // Arrange
        var existingTask = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5 };
        _taskRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTaskCommand(1), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _taskRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ManagedTask?)null);
        var handler = new DeleteTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTaskCommand(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenDeleteFails()
    {
        // Arrange
        var existingTask = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5 };
        _taskRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTaskCommand(1), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
