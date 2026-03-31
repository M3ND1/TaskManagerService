using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Infrastructure.Data.Database;
using TaskManager.Infrastructure.Repositories;

namespace Infrastructure.IntegrationTests;

public class ManagedTaskTagRepositoryTests : IDisposable
{
    private readonly ManagedTaskRepository _repository;
    private readonly TaskManagerDbContext _context;
    private readonly SqliteConnection _sqliteConnection;

    public ManagedTaskTagRepositoryTests()
    {
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        _sqliteConnection.Open();
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        _context = new TaskManagerDbContext(options);
        _context.Database.EnsureCreated();
        _context.Users.AddRange(Helpers.CreateUsers());
        _context.SaveChanges();

        _context.Tags.AddRange(
            new Tag { Id = 1, Name = "Bug", CreatedById = 1 },
            new Tag { Id = 2, Name = "Feature", CreatedById = 1 });
        _context.ManagedTasks.Add(new ManagedTask
        {
            Id = 10, Title = "Test Task", Description = "Desc",
            Priority = PriorityLevel.Medium, CreatedById = 1
        });
        _context.SaveChanges();

        _repository = new ManagedTaskRepository(_context);
    }

    [Fact]
    public async Task GetWithTagsAsync_Should_ReturnTask_WithEmptyTagsCollection()
    {
        var task = await _repository.GetWithTagsAsync(10);

        task.Should().NotBeNull();
        task!.Tags.Should().NotBeNull();
        task.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWithTagsAsync_Should_ReturnNull_WhenTaskDoesNotExist()
    {
        var task = await _repository.GetWithTagsAsync(999);
        task.Should().BeNull();
    }

    [Fact]
    public async Task AssignAndRetrieveTags_Should_PersistRelationship()
    {
        // Arrange - load tracked task and tag, assign via collection
        var task = await _context.ManagedTasks.Include(t => t.Tags).FirstAsync(t => t.Id == 10);
        var tag = await _context.Tags.FindAsync(1);

        task.Tags!.Add(tag!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var reloaded = await _repository.GetWithTagsAsync(10);

        // Assert
        reloaded!.Tags.Should().HaveCount(1);
        reloaded.Tags!.First().Name.Should().Be("Bug");
    }

    [Fact]
    public async Task AssignMultipleTags_Should_PersistAllRelationships()
    {
        // Arrange
        var task = await _context.ManagedTasks.Include(t => t.Tags).FirstAsync(t => t.Id == 10);
        var tag1 = await _context.Tags.FindAsync(1);
        var tag2 = await _context.Tags.FindAsync(2);

        task.Tags!.Add(tag1!);
        task.Tags.Add(tag2!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var reloaded = await _repository.GetWithTagsAsync(10);

        // Assert
        reloaded!.Tags.Should().HaveCount(2);
        reloaded.Tags!.Select(t => t.Name).Should().BeEquivalentTo(["Bug", "Feature"]);
    }

    [Fact]
    public async Task RemoveTag_Should_DeleteRelationship()
    {
        // Arrange - assign first
        var task = await _context.ManagedTasks.Include(t => t.Tags).FirstAsync(t => t.Id == 10);
        var tag = await _context.Tags.FindAsync(1);
        task.Tags!.Add(tag!);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Remove the tag via UpdateAsync
        var loadedTask = await _context.ManagedTasks.Include(t => t.Tags).FirstAsync(t => t.Id == 10);
        var tagToRemove = loadedTask.Tags!.First(t => t.Id == 1);
        loadedTask.Tags!.Remove(tagToRemove);
        await _repository.UpdateAsync(loadedTask);
        _context.ChangeTracker.Clear();

        // Act
        var reloaded = await _repository.GetWithTagsAsync(10);

        // Assert
        reloaded!.Tags.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
        _sqliteConnection.Dispose();
    }
}
