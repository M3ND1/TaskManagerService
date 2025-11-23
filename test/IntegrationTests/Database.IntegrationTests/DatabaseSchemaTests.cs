using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using TaskManager.Infrastructure;

namespace Database.IntegrationTests;

public class DatabaseSchemaTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TaskManagerDbContext> _options;
    private readonly TaskManagerDbContext _context;

    public DatabaseSchemaTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TaskManagerDbContext(_options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void Database_Should_HaveAllExpectedTables()
    {
        var tables = GetAllTables();

        tables.Should().Contain("Users");
        tables.Should().Contain("ManagedTasks");
        tables.Should().Contain("Tags");
    }

    [Fact]
    public void Users_Table_Should_HaveCorrectColumns()
    {
        var columns = GetTableColumns("Users");

        columns.Should().Contain("Id");
        columns.Should().Contain("FirstName");
        columns.Should().Contain("LastName");
        columns.Should().Contain("Email");
        columns.Should().Contain("Username");
        columns.Should().Contain("PasswordHash");
        columns.Should().Contain("IsActive");
        columns.Should().Contain("CreatedAt");
        columns.Should().Contain("LastLoginAt");
        columns.Should().Contain("UpdatedAt");
    }

    [Fact]
    public void ManagedTasks_Table_Should_HaveCorrectColumns()
    {
        var columns = GetTableColumns("ManagedTasks");

        columns.Should().Contain("Id");
        columns.Should().Contain("Title");
        columns.Should().Contain("Description");
        columns.Should().Contain("CreatedAt");
        columns.Should().Contain("UpdatedAt");
        columns.Should().Contain("DueDate");
        columns.Should().Contain("CompletedAt");
        columns.Should().Contain("Priority");
        columns.Should().Contain("IsCompleted");
        columns.Should().Contain("AssignedToId");
        columns.Should().Contain("CreatedById");
    }

    private List<string> GetAllTables()
    {
        var tables = new List<string>();
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'ef_%';";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private List<string> GetTableColumns(string tableName)
    {
        var columns = new List<string>();
        using var command = _connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
