using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Repositories;

namespace Infrastructure.IntegrationTests;

public class ManagedTaskRepositoryTests : IDisposable
{
    private readonly ManagedTaskRepository _repository;
    private readonly TaskManagerDbContext _context;
    private readonly int _managedTasksInDatabase;
    private readonly SqliteConnection _sqliteConnection;
    public ManagedTaskRepositoryTests()
    {
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        _sqliteConnection.Open();
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        _context = new TaskManagerDbContext(options);
        _context.Database.EnsureCreated();
        _context.ManagedTasks.AddRange(Helpers.CreateManagedTasks());
        _context.Users.AddRange(Helpers.CreateUsers());
        _context.SaveChanges();
        _repository = new ManagedTaskRepository(_context);
        _managedTasksInDatabase = _context.ManagedTasks.Count();
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public async Task GetAsync_Should_ReturnNull(int id)
    {
        //Arrange
        var managedTask = await _repository.GetAsync(id);
        //Act
        managedTask.Should().BeNull();
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetAsync_Should_ReturnCorrectTasks(int id)
    {
        var managedTask = await _repository.GetAsync(id);

        managedTask.Should().NotBeNull();
        managedTask!.Id.Should().BeOneOf([1, 2, 3, 4]);
    }
    [Fact]
    public async Task AddAsync_Should_ReturnException()
    {
        Func<Task> act = async () => await _repository.AddAsync(new ManagedTask());

        await act.Should().ThrowAsync<Exception>();
    }
    [Fact]
    public async Task AddAsync_Should_AddManagedTask()
    {
        //Arrange
        var newTask = new ManagedTask
        {
            Title = "New Test Task",
            Description = "Test description",
            CreatedAt = DateTime.UtcNow,
            Priority = PriorityLevel.Medium,
            IsCompleted = false,
            CreatedById = 1
        };
        //Act
        bool cut = await _repository.AddAsync(newTask);

        //Assert
        cut.Should().BeTrue();
        _context.ManagedTasks.Count().Should().BeGreaterThan(_managedTasksInDatabase);
        var fromDb = await _context.ManagedTasks.FirstOrDefaultAsync(
            t => t.CreatedAt == newTask.CreatedAt &&
            t.Description == newTask.Description);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateCorrectTaskTitle()
    {
        _context.ChangeTracker.Clear();
        var mt = new ManagedTask
        {
            Id = 1,
            Title = "SuperSecretTitle",
            CreatedById = 1
        };
        bool cut = await _repository.UpdateAsync(mt);

        cut.Should().Be(true);
        var fromDb = await _context.ManagedTasks.FirstOrDefaultAsync(t => t.Id == mt.Id, CancellationToken.None);
        fromDb.Should().NotBeNull();
        fromDb.Id.Should().Be(1);
        fromDb.Title.Should().Be("SuperSecretTitle");
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateCorrectTaskWholeProperties()
    {
        _context.ChangeTracker.Clear();
        var mt = new ManagedTask
        {
            Id = 1,
            Title = "Title123",
            Description = "Description123123123",
            UpdatedAt = DateTime.UtcNow,
            Priority = PriorityLevel.High,
            IsCompleted = true,
            EstimatedHours = 5,
            CreatedById = 1
        };
        bool cut = await _repository.UpdateAsync(mt);
        _context.ChangeTracker.Clear();

        cut.As<bool>().Should().Be(true);
        var fromDb = await _context.ManagedTasks.FirstOrDefaultAsync(t => t.Id == mt.Id);
        fromDb.Should().NotBe(null);
        fromDb.Title.Should().Be("Title123");
        fromDb.Description.Should().Be("Description123123123");
        fromDb.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        fromDb.Priority.Should().Be(PriorityLevel.High);
        fromDb.EstimatedHours.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse()
    {
        _context.ChangeTracker.Clear();
        var nonExistentTask = new ManagedTask
        {
            Id = 999,
            Title = "Non-existent task",
            CreatedById = 1
        };
        var result = await _repository.UpdateAsync(nonExistentTask);
        result.As<bool>().Should().Be(false);
    }
    [Fact]
    public async Task DeleteAsync_Should_DeleteTask()
    {
        var result = await _repository.DeleteAsync(1);
        result.As<bool>().Should().Be(true);
        var fromDb = await _context.ManagedTasks.FirstOrDefaultAsync(t => t.Id == 1);
        fromDb.Should().BeNull();
    }
    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    public async Task DeleteAsync_Should_ReturnFalse(int id)
    {
        var result = await _repository.DeleteAsync(id);
        result.As<bool>().Should().Be(false);
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}