using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Infrastructure.Data.Database;
using TaskManager.Infrastructure.Repositories;

namespace Infrastructure.IntegrationTests;

public class TagRepositoryTests : IDisposable
{
    private readonly TagRepository _repository;
    private readonly TaskManagerDbContext _context;
    private readonly SqliteConnection _sqliteConnection;

    public TagRepositoryTests()
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
        _repository = new TagRepository(_context);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenTagDoesNotExist()
    {
        var tag = await _repository.GetAsync(999);
        tag.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_Should_PersistTag_AndReturnTrue()
    {
        var tag = new Tag { Name = "Bug", Color = "#FF0000", CreatedById = 1 };

        var result = await _repository.AddAsync(tag);

        result.Should().BeTrue();
        tag.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnTag_AfterAdd()
    {
        var tag = new Tag { Name = "Feature", CreatedById = 1 };
        await _repository.AddAsync(tag);

        var fetched = await _repository.GetAsync(tag.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Feature");
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllTags()
    {
        await _repository.AddAsync(new Tag { Name = "Tag1", CreatedById = 1 });
        await _repository.AddAsync(new Tag { Name = "Tag2", CreatedById = 1 });

        var all = await _repository.GetAllAsync();

        all.Should().HaveCount(2);
        all.Select(t => t.Name).Should().BeEquivalentTo(["Tag1", "Tag2"]);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnEmptyList_WhenNoTagsExist()
    {
        var all = await _repository.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistChanges()
    {
        var tag = new Tag { Name = "OldName", CreatedById = 1 };
        await _repository.AddAsync(tag);

        tag.Name = "NewName";
        var result = await _repository.UpdateAsync(tag);

        result.Should().BeTrue();
        var fetched = await _repository.GetAsync(tag.Id);
        fetched!.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTag_AndReturnTrue()
    {
        var tag = new Tag { Name = "ToDelete", CreatedById = 1 };
        await _repository.AddAsync(tag);

        var result = await _repository.DeleteAsync(tag.Id);

        result.Should().BeTrue();
        (await _repository.GetAsync(tag.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenTagDoesNotExist()
    {
        var result = await _repository.DeleteAsync(999);
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        _sqliteConnection.Dispose();
    }
}
