using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.Features.Tasks.Queries.GetAllTasks;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tasks;

public class GetAllTasksQueryHandlerTests
{
    private readonly Mock<IManagedTaskRepository> _repoMock = new();
    private readonly IMapper _mapper;
    private readonly GetAllTasksQueryHandler _handler;

    public GetAllTasksQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new GetAllTasksQueryHandler(_repoMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult_WithCorrectMetadata()
    {
        var tasks = new List<ManagedTask>
        {
            new() { Id = 1, Title = "Task 1", CreatedBy = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = "h" } },
            new() { Id = 2, Title = "Task 2", CreatedBy = new User { Id = 1, Username = "user1", Email = "u1@test.com", PasswordHash = "h" } }
        };
        _repoMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks.AsEnumerable(), 25));

        var result = await _handler.Handle(new GetAllTasksQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PassesFilterParameters_ToRepository()
    {
        _repoMock.Setup(r => r.GetPagedAsync(2, 5, true, PriorityLevel.High, 42, "urgent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ManagedTask>(), 0));

        var query = new GetAllTasksQuery(
            PageNumber: 2, PageSize: 5,
            IsCompleted: true, Priority: PriorityLevel.High,
            AssignedToId: 42, Search: "urgent");

        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetPagedAsync(2, 5, true, PriorityLevel.High, 42, "urgent", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MapsEntityFields_ToDto()
    {
        var user = new User { Id = 5, Username = "alice", Email = "alice@test.com", PasswordHash = "h" };
        var task = new ManagedTask
        {
            Id = 10, Title = "Important", Description = "Do it",
            Priority = PriorityLevel.High, IsCompleted = true,
            EstimatedHours = 4, ActualHours = 6,
            CreatedById = 5, CreatedBy = user, AssignedTo = user
        };
        _repoMock.Setup(r => r.GetPagedAsync(1, 20, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { task }.AsEnumerable(), 1));

        var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

        var dto = result.Items.Single();
        dto.Id.Should().Be(10);
        dto.Title.Should().Be("Important");
        dto.Priority.Should().Be(PriorityLevel.High);
        dto.IsCompleted.Should().BeTrue();
        dto.CreatedBy!.Username.Should().Be("alice");
        dto.AssignedTo!.Username.Should().Be("alice");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        _repoMock.Setup(r => r.GetPagedAsync(1, 20, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ManagedTask>(), 0));

        var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DefaultParameters_UsePageOneAndSize20()
    {
        _repoMock.Setup(r => r.GetPagedAsync(1, 20, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ManagedTask>(), 0));

        await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

        _repoMock.Verify(r => r.GetPagedAsync(1, 20, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
