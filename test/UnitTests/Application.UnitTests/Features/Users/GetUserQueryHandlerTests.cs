using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Queries.GetUser;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Users;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly IMapper _mapper;

    public GetUserQueryHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnUserResponseDto_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Username = "johndoe",
            IsActive = true,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        _userRepositoryMock.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = new GetUserQueryHandler(_userRepositoryMock.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetUserQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.Username.Should().Be("johndoe");
        result.IsActive.Should().BeTrue();
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = new GetUserQueryHandler(_userRepositoryMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new GetUserQuery(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
