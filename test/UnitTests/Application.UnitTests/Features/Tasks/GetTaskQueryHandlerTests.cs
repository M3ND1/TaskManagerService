using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.Features.Tasks.Queries.GetTask;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tasks;

public class GetTaskQueryHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();
    private readonly IMapper _mapper;

    public GetTaskQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnManagedTaskResponseDto_WhenTaskExists()
    {
        // Arrange
        var task = new ManagedTask
        {
            Id = 1,
            Title = "Fix bug",
            Description = "Critical bug in login",
            Priority = PriorityLevel.High,
            IsCompleted = false,
            CreatedById = 5,
            EstimatedHours = 3
        };
        _taskRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new GetTaskQueryHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetTaskQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Fix bug");
        result.Description.Should().Be("Critical bug in login");
        result.Priority.Should().Be(PriorityLevel.High);
        result.EstimatedHours.Should().Be(3);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ManagedTask?)null);
        var handler = new GetTaskQueryHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new GetTaskQuery(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
