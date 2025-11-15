using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Core.Entities;

namespace TaskManager.Infrastructure.IntegrationTests
{
    public class CancellationTokenTests : IDisposable
    {
        private readonly TaskManagerDbContext _context;
        private readonly UserRepository _userRepository;

        public CancellationTokenTests()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskManagerDbContext(options);
            _userRepository = new UserRepository(_context);
        }

        [Fact]
        public async Task GetAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            // Arrange
            var user = new User { FirstName = "Test", LastName = "User", Email = "test@test.com", Username = "testuser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var cancellationToken = new CancellationToken(canceled: true);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _userRepository.GetAsync(user.Id, cancellationToken)
            );
        }

        [Fact]
        public async Task GetAsync_WithValidToken_ReturnsUser()
        {
            // Arrange
            var user = new User { FirstName = "Test", LastName = "User", Email = "test@test.com", Username = "testuser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var cancellationToken = new CancellationToken(canceled: false);

            // Act
            var result = await _userRepository.GetAsync(user.Id, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task AddAsync_WithCancelledToken_ThrowsTaskCanceledException()
        {
            // Arrange
            var user = new User { FirstName = "Test", LastName = "User", Email = "test@test.com", Username = "testuser", PasswordHash = "hash" };
            var cancellationToken = new CancellationToken(canceled: true);

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => _userRepository.AddAsync(user, cancellationToken)
            );
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}