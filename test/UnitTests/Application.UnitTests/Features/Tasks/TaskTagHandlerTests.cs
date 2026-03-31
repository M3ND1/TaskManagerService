using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.Features.Tasks.Commands.AssignTagToTask;
using TaskManager.Application.Features.Tasks.Commands.RemoveTagFromTask;
using TaskManager.Application.Features.Tasks.Queries.GetTaskTags;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tasks;

public class GetTaskTagsQueryHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();
    private readonly IMapper _mapper;

    public GetTaskTagsQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnTags_WhenTaskExists()
    {
        // Arrange
        var task = new ManagedTask
        {
            Id = 1, Title = "Task", CreatedById = 1,
            Tags = [new Tag { Id = 10, Name = "Bug" }, new Tag { Id = 11, Name = "Feature" }]
        };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new GetTaskTagsQueryHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetTaskTagsQuery(1), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo(["Bug", "Feature"]);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenTaskHasNoTags()
    {
        // Arrange
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 1, Tags = [] };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new GetTaskTagsQueryHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetTaskTagsQuery(1), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ManagedTask?)null);
        var handler = new GetTaskTagsQueryHandler(_taskRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new GetTaskTagsQuery(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class AssignTagToTaskCommandHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly IMapper _mapper;

    public AssignTagToTaskCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdatedTags_WhenAssignSucceeds()
    {
        // Arrange
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag>() };
        var tag = new Tag { Id = 10, Name = "Bug" };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new AssignTagToTaskCommandHandler(_taskRepositoryMock.Object, _tagRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new AssignTagToTaskCommand(1, 10, UserId: 5), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Bug");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ManagedTask?)null);
        var handler = new AssignTagToTaskCommandHandler(_taskRepositoryMock.Object, _tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new AssignTagToTaskCommand(99, 10, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_WhenUserIsNotOwner()
    {
        // Arrange
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag>() };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new AssignTagToTaskCommandHandler(_taskRepositoryMock.Object, _tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new AssignTagToTaskCommand(1, 10, UserId: 99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTagDoesNotExist()
    {
        // Arrange
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag>() };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tag?)null);
        var handler = new AssignTagToTaskCommandHandler(_taskRepositoryMock.Object, _tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new AssignTagToTaskCommand(1, 99, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenTagAlreadyAssigned()
    {
        // Arrange
        var tag = new Tag { Id = 10, Name = "Bug" };
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag> { tag } };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _tagRepositoryMock.Setup(r => r.GetAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        var handler = new AssignTagToTaskCommandHandler(_taskRepositoryMock.Object, _tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new AssignTagToTaskCommand(1, 10, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}

public class RemoveTagFromTaskCommandHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _taskRepositoryMock = new();

    [Fact]
    public async Task Handle_Should_RemoveTag_Successfully()
    {
        // Arrange
        var tag = new Tag { Id = 10, Name = "Bug" };
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag> { tag } };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ManagedTask>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new RemoveTagFromTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new RemoveTagFromTaskCommand(1, 10, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        task.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ManagedTask?)null);
        var handler = new RemoveTagFromTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new RemoveTagFromTaskCommand(99, 10, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_WhenUserIsNotOwner()
    {
        // Arrange
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag>() };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new RemoveTagFromTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new RemoveTagFromTaskCommand(1, 10, UserId: 99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTagNotAssignedToTask()
    {
        // Arrange
        var task = new ManagedTask { Id = 1, Title = "Task", CreatedById = 5, Tags = new List<Tag>() };
        _taskRepositoryMock.Setup(r => r.GetWithTagsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new RemoveTagFromTaskCommandHandler(_taskRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new RemoveTagFromTaskCommand(1, 99, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
