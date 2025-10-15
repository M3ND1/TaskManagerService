using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using TaskManager.Infrastructure;

namespace Database.IntegrationTests;

public class MigrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TaskManagerDbContext> _options;

    public MigrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite(_connection)
            .Options;
    }

    [Fact]
    public void ApplyMigrations_ShouldSucceed()
    {
        // Arrange & Act
        Action applyMigrations = () =>
        {
            using var context = new TaskManagerDbContext(_options);
            context.Database.EnsureCreated();
        };

        // Assert
        applyMigrations.Should().NotThrow();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
