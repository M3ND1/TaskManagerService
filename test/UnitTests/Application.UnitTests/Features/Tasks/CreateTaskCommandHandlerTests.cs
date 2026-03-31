using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.Features.Tasks.Commands.CreateTask;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();
    private readonly IMapper _mapper;

    public CreateTaskCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnManagedTaskResponseDto_WhenTaskCreatedSuccessfully()
    {
        // Arrange
        var dto = new CreateManagedTaskDto
        {
            Title = "Fix login bug",
            Description = "Users cannot login with email",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityLevel.High,
            EstimatedHours = 4
        };
        var command = new CreateTaskCommand(dto, UserId: 1);
        _taskRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Fix login bug");
        result.Description.Should().Be("Users cannot login with email");
        result.Priority.Should().Be(PriorityLevel.High);
        result.EstimatedHours.Should().Be(4);
        result.CreatedById.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_SetAssignedToIdAndCreatedById_FromUserId()
    {
        // Arrange
        ManagedTask? capturedTask = null;
        _taskRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>()))
            .Callback<ManagedTask, CancellationToken>((t, _) => capturedTask = t)
            .ReturnsAsync(true);
        var handler = new CreateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        await handler.Handle(new CreateTaskCommand(new CreateManagedTaskDto { Title = "Task", Description = "Desc" }, UserId: 42), CancellationToken.None);

        // Assert
        capturedTask.Should().NotBeNull();
        capturedTask!.AssignedToId.Should().Be(42);
        capturedTask.CreatedById.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenAddFails()
    {
        // Arrange
        var dto = new CreateManagedTaskDto { Title = "Task", Description = "Desc" };
        _taskRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new CreateTaskCommandHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new CreateTaskCommand(dto, UserId: 1), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
