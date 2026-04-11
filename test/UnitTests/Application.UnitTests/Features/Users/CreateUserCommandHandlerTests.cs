using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands.CreateUser;
using TaskManager.Application.Mappings;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly IMapper _mapper;

    public CreateUserCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_ReturnUserResponseDto_WhenUserCreatedSuccessfully()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Username = "johndoe",
            Password = "SecurePass1!",
            ConfirmPassword = "SecurePass1!"
        };
        var command = new CreateUserCommand(dto);
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync(dto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _passwordServiceMock.Setup(p => p.SecurePassword(dto.Password)).Returns("hashed_password");
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.Username.Should().Be("johndoe");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_HashPassword_BeforeStoringUser()
    {
        // Arrange
        User? capturedUser = null;
        var dto = new CreateUserDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Username = "janedoe",
            Password = "SecurePass1!",
            ConfirmPassword = "SecurePass1!"
        };
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync(dto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync(true);
        _passwordServiceMock.Setup(p => p.SecurePassword("SecurePass1!")).Returns("argon2_hash_result");
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _mapper);

        // Act
        await handler.Handle(new CreateUserCommand(dto), CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("argon2_hash_result");
        _passwordServiceMock.Verify(p => p.SecurePassword("SecurePass1!"), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new CreateUserDto { Email = "taken@example.com", Username = "newuser", Password = "P@ss1234" };
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("taken@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new CreateUserCommand(dto), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Email*already taken*");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var dto = new CreateUserDto { Email = "new@example.com", Username = "takenuser", Password = "P@ss1234" };
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("takenuser", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new CreateUserCommand(dto), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Username*already taken*");
    }

    [Fact]
    public async Task Handle_Should_ThrowDatabaseOperationException_WhenAddFails()
    {
        // Arrange
        var dto = new CreateUserDto { Email = "john@example.com", Username = "johndoe", Password = "P@ss1234" };
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync(dto.Username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordServiceMock.Setup(p => p.SecurePassword(It.IsAny<string>())).Returns("hash");
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(new CreateUserCommand(dto), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DatabaseOperationException>();
    }

    [Fact]
    public async Task Handle_Should_NotCheckUsername_WhenEmailAlreadyTaken()
    {
        // Arrange
        var dto = new CreateUserDto { Email = "taken@example.com", Username = "user", Password = "P@ss1234" };
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("taken@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _passwordServiceMock.Object, _mapper);

        // Act
        try { await handler.Handle(new CreateUserCommand(dto), CancellationToken.None); } catch { }

        // Assert
        _userRepositoryMock.Verify(r => r.UsernameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
