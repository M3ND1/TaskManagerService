using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Application.Features.Tags.Queries.GetAllTags;
using TaskManager.Application.Features.Tags.Queries.GetTag;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tags;

public class GetTagQueryHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly IMapper _mapper;

    public GetTagQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnTagResponseDto_WhenTagExists()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Bug", Color = "#FF0000", CreatedAt = DateTime.UtcNow, CreatedById = 1 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        var handler = new GetTagQueryHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetTagQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Bug");
        result.Color.Should().Be("#FF0000");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTagDoesNotExist()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tag?)null);
        var handler = new GetTagQueryHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new GetTagQuery(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class GetAllTagsQueryHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly IMapper _mapper;

    public GetAllTagsQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnAllTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "Bug", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Feature", CreatedAt = DateTime.UtcNow }
        };
        _tagRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tags);
        var handler = new GetAllTagsQueryHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo(["Bug", "Feature"]);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoTagsExist()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = new GetAllTagsQueryHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
