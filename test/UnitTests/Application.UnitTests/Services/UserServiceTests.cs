using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IPasswordService> _passwordService;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator;
        private readonly Mock<ITokenValidationService> _tokenValidationService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;
            _userRepository = new Mock<IUserRepository>();
            _passwordService = new Mock<IPasswordService>();
            _refreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _tokenValidationService = new Mock<ITokenValidationService>();
            _userService = new UserService(_userRepository.Object,
                _mapper,
                _passwordService.Object,
                _tokenValidationService.Object,
                _refreshTokenRepository.Object,
                _jwtTokenGenerator.Object);
        }

        [Fact]
        public async Task CreateUserAsync_Should_Return_Dto()
        {
            // Arrange
            _userRepository.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var createUserDto = new CreateUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Username = "johndoe",
                Password = "password123",
                PhoneNumber = "123-456-7890"
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<UserResponseDto>();
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john.doe@example.com");
            result.Username.Should().Be("johndoe");
            result.PhoneNumber.Should().Be("123-456-7890");
        }

        [Fact]
        public async Task CreateUserAsync_Should_Return_Null_When_Repository_Fails()
        {
            // Arrange
            _userRepository.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var createUserDto = new CreateUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Username = "johndoe",
                Password = "password123"
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserAsync_Should_Map_DTO_Properties_Correctly()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Username = "janesmith",
                Password = "securepassword",
                PhoneNumber = "987-654-3210"
            };
            User? capturedUser = null;
            _userRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((user, cancellationToken) => capturedUser = user)
                .ReturnsAsync(true);

            // Act
            await _userService.CreateUserAsync(createUserDto);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser?.FirstName.Should().Be("Jane");
            capturedUser?.LastName.Should().Be("Smith");
            capturedUser?.Email.Should().Be("jane.smith@example.com");
            capturedUser?.Username.Should().Be("janesmith");
            capturedUser?.PhoneNumber.Should().Be("987-654-3210");
        }

        [Fact]
        public async Task GetUserAsync_Should_Return_Mapped_DTO_When_User_Exists()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Username = "johndoe",
                PhoneNumber = "123-456-7890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            };
            _userRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<UserResponseDto>();
            result.Id.Should().Be(1);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john.doe@example.com");
            result.Username.Should().Be("johndoe");
            result.PhoneNumber.Should().Be("123-456-7890");
            result.IsActive.Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999999)]
        public async Task GetUserAsync_Should_Return_Null_When_No_User(int userId)
        {
            // Arrange
            _userRepository.Setup(x => x.GetAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_True_When_User_Updated_And_Update_DateTime_Field()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Old Name",
                LastName = "Old LastName",
                Email = "old.email@example.com",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            User? capturedUser = null;
            _userRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
            _userRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((user, cancellationToken) => capturedUser = user)
                .ReturnsAsync(true);

            var updateDto = new UpdateUserDto
            {
                FirstName = "New Name",
                LastName = "New LastName",
                Email = "new.email@example.com"
            };

            // Act
            var result = await _userService.UpdateUserAsync(1, updateDto);

            // Assert
            result.Should().BeTrue();
            capturedUser.Should().NotBeNull();
            capturedUser.FirstName.Should().Be("New Name");
            capturedUser.LastName.Should().Be("New LastName");
            capturedUser.Email.Should().Be("new.email@example.com");
            capturedUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            capturedUser.UpdatedAt.Should().BeOnOrAfter(existingUser.UpdatedAt.Value);
            _userRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => u.FirstName == "New Name" && u.Email == "new.email@example.com"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            _userRepository.Setup(x => x.GetAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
            var updateDto = new UpdateUserDto
            {
                FirstName = "Updated Name"
            };

            // Act
            var result = await _userService.UpdateUserAsync(999, updateDto);

            // Assert
            result.Should().BeFalse();
            _userRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_False_When_Repository_Update_Fails()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "Name",
                Email = "existing.email@example.com"
            };
            _userRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
            _userRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var updateDto = new UpdateUserDto
            {
                FirstName = "UpdatedName"
            };

            // Act
            var result = await _userService.UpdateUserAsync(1, updateDto);

            // Assert
            result.Should().BeFalse();
            _userRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Map_All_DTO_Properties_Correctly()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "ExistingFirstName",
                LastName = "ExistingLastName",
                Email = "email@example.com",
                Username = "username",
                PhoneNumber = "000-000-0000"
            };
            User? capturedUser = null;
            _userRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
            _userRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((user, cancellationToken) => capturedUser = user)
                .ReturnsAsync(true);

            var updateDto = new UpdateUserDto
            {
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Email = "email@example.com",
                Username = "updatedUsername",
                PhoneNumber = "111-111-1111"
            };

            // Act
            var result = await _userService.UpdateUserAsync(1, updateDto);

            // Assert
            result.Should().BeTrue();
            capturedUser.Should().NotBeNull();
            capturedUser.FirstName.Should().Be("UpdatedFirstName");
            capturedUser.LastName.Should().Be("UpdatedLastName");
            capturedUser.Email.Should().Be("email@example.com");
            capturedUser.Username.Should().Be("updatedUsername");
            capturedUser.PhoneNumber.Should().Be("111-111-1111");
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_True_When_User_Deleted_Successfully()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "John"
            };
            _userRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
            _userRepository.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(1);

            // Assert
            result.Should().BeTrue();
            _userRepository.Verify(x => x.GetAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _userRepository.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            _userRepository.Setup(x => x.GetAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.DeleteUserAsync(999);

            // Assert
            result.Should().BeFalse();
            _userRepository.Verify(x => x.GetAsync(999, It.IsAny<CancellationToken>()), Times.Once);
            _userRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_False_When_Repository_Delete_Fails()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "John"
            };
            _userRepository.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
            _userRepository.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync(1);

            // Assert
            result.Should().BeFalse();
            _userRepository.Verify(x => x.GetAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _userRepository.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task DeleteUserAsync_Should_Return_False_For_Invalid_User_Ids(int invalidId)
        {
            // Arrange
            _userRepository.Setup(x => x.GetAsync(invalidId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.DeleteUserAsync(invalidId);

            // Assert
            result.Should().BeFalse();
            _userRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}