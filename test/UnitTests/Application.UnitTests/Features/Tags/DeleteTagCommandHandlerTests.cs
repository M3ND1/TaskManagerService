using FluentAssertions;
using Moq;
using TaskManager.Application.Features.Tags.Commands.DeleteTag;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Tags;

public class DeleteTagCommandHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();

    [Fact]
    public async Task Handle_Should_DeleteTag_Successfully()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "Bug", CreatedById = 5 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        _tagRepositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteTagCommandHandler(_tagRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTagCommand(1, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _tagRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenTagDoesNotExist()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tag?)null);
        var handler = new DeleteTagCommandHandler(_tagRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTagCommand(99, UserId: 1), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_WhenUserIsNotOwner()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "Bug", CreatedById = 5 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        var handler = new DeleteTagCommandHandler(_tagRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTagCommand(1, UserId: 99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenDeleteFails()
    {
        // Arrange
        var existingTag = new Tag { Id = 1, Name = "Bug", CreatedById = 5 };
        _tagRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTag);
        _tagRepositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteTagCommandHandler(_tagRepositoryMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(new DeleteTagCommand(1, UserId: 5), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }
}
