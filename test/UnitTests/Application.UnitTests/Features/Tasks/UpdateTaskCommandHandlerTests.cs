using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.Features.Tasks.Commands.UpdateTask;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tasks;

public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdatedTaskResponseDto_WhenUpdateSucceeds()
    {
        // Arrange
        var existingTask = new ManagedTask { Id = 1, Title = "Old Title", Description = "Old Desc", CreatedById = 5 };
        var dto = new UpdateManagedTaskDto { Title = "New Title", Priority = PriorityLevel.High };
        _taskRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new UpdateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new UpdateTaskCommand(1, dto), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Title");
        result.Priority.Should().Be(PriorityLevel.High);
    }

    [Fact]
    public async Task Handle_Should_SetUpdatedAt_ToUtcNow()
    {
        // Arrange
        var existingTask = new ManagedTask { Id = 1, Title = "Task", Description = "Desc", CreatedById = 5 };
        _taskRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new UpdateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        await handler.Handle(new UpdateTaskCommand(1, new UpdateManagedTaskDto()), CancellationToken.None);

        // Assert
        existingTask.UpdatedAt.Should().NotBeNull();
        existingTask.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ManagedTask?)null);
        var handler = new UpdateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateTaskCommand(99, new UpdateManagedTaskDto()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenUpdateFails()
    {
        // Arrange
        var existingTask = new ManagedTask { Id = 1, Title = "Task", Description = "Desc", CreatedById = 5 };
        _taskRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new UpdateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateTaskCommand(1, new UpdateManagedTaskDto()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
