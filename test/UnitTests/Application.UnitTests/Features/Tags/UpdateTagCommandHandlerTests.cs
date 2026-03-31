using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Application.Features.Tags.Commands.UpdateTag;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tags;

public class UpdateTagCommandHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly IMapper _mapper;

    public UpdateTagCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdatedTagResponseDto_WhenUpdateSucceeds()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "OldName", CreatedById = 5 };
        var dto = new UpdateTagDto { Name = "NewName", Color = "#00FF00" };
        var command = new UpdateTagCommand(1, dto, UserId: 5);
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        _tagRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new UpdateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("NewName");
        result.Color.Should().Be("#00FF00");
    }

    [Fact]
    public async Task Handle_Should_SetUpdatedAt_ToUtcNow()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "Name", CreatedById = 5 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        _tagRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new UpdateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        await handler.Handle(new UpdateTagCommand(1, new UpdateTagDto(), UserId: 5), CancellationToken.None);

        // Assert
        existingTag.UpdatedAt.Should().NotBeNull();
        existingTag.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTagDoesNotExist()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tag?)null);
        var handler = new UpdateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateTagCommand(99, new UpdateTagDto(), UserId: 1), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_WhenUserIsNotOwner()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "OldName", CreatedById = 5 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        var handler = new UpdateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateTagCommand(1, new UpdateTagDto { Name = "Hack" }, UserId: 99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenUpdateFails()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "Name", CreatedById = 5 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        _tagRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new UpdateTagCommandHandler(_tagRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new UpdateTagCommand(1, new UpdateTagDto(), UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
