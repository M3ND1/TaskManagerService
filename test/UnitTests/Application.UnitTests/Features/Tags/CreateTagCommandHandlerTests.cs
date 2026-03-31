using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Application.Features.Tags.Commands.CreateTag;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tags;

public class CreateTagCommandHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly IMapper _mapper;

    public CreateTagCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnTagResponseDto_WhenTagCreatedSuccessfully()
    {
        // Arrange
        var dto = new CreateTagDto { Name = "Bug", Color = "#FF0000", Description = "Bug tag" };
        var command = new CreateTagCommand(dto, UserId: 1);
        _tagRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Bug");
        result.Color.Should().Be("#FF0000");
        result.CreatedById.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenAddFails()
    {
        // Arrange
        var dto = new CreateTagDto { Name = "Bug" };
        var command = new CreateTagCommand(dto, UserId: 1);
        _tagRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new CreateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }

    [Fact]
    public async Task Handle_Should_SetCreatedById_FromCommand()
    {
        // Arrange
        Tag? capturedTag = null;
        _tagRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .Callback<Tag, CancellationToken>((t, _) => capturedTag = t)
            .ReturnsAsync(true);
        var handler = new CreateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        await handler.Handle(new CreateTagCommand(new CreateTagDto { Name = "Test" }, UserId: 42), CancellationToken.None);

        // Assert
        capturedTag.Should().NotBeNull();
        capturedTag!.CreatedById.Should().Be(42);
    }
}
