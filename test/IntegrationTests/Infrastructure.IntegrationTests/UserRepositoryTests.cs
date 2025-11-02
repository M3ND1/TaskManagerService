
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TaskManager.Core.Entities;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Repositories;

namespace Infrastructure.IntegrationTests;

public class UserRepositoryTests : IDisposable
{
    private readonly UserRepository _repository;
    private readonly TaskManagerDbContext _context;
    private readonly int _usersInDatabase;
    private SqliteConnection _sqliteConnection;

    public UserRepositoryTests()
    {
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        _sqliteConnection.Open();

        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        _context = new TaskManagerDbContext(options);
        _context.Database.EnsureCreated();
        _context.Users.AddRange(Helpers.CreateUsers());
        _context.ManagedTasks.AddRange(Helpers.CreateManagedTasks());
        _context.SaveChanges();
        _repository = new UserRepository(_context);
        _usersInDatabase = _context.Users.Count();
    }
    [Fact]
    public async Task AddAsync_Should_AddWholeUser()
    {
        User user = new()
        {
            FirstName = "FirstNameTests",
            LastName = "LastNameTest",
            Email = "TestEmail",
            PhoneNumber = "123123123",
            Username = "TestUsername",
            PasswordHash = "TestHash",
        };

        var result = await _repository.AddAsync(user);

        result.Should().BeTrue();
        _context.Users.Count().Should().BeGreaterThan(_usersInDatabase);

        var fromDb = await _context.Users.FirstOrDefaultAsync(u =>
            u.FirstName == user.FirstName &&
            u.LastName == user.LastName &&
            u.Email == user.Email &&
            u.PhoneNumber == user.PhoneNumber &&
            u.Username == user.Username);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_Should_ReturnFalse_When_User_Is_Null()
    {
        var result = await _repository.AddAsync(null);

        result.Should().BeFalse();
        _context.Users.Count().Should().Be(_usersInDatabase);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99999999)]
    [InlineData(0)]
    public async Task GetAsync_Should_ReturnNull_When_Id_Is_Invalid(int id)
    {
        var result = await _repository.GetAsync(id);

        result.Should().BeNull();
    }
    [Fact]
    public async Task GetAsync_Should_Return_Correct_User()
    {
        var existingUser = _context.Users.First();
        var result = await _repository.GetAsync(1);

        result.Should().NotBeNull();
        result.Id.Should().Be(existingUser.Id);
        result.FirstName.Should().Be(existingUser.FirstName);
        result.LastName.Should().Be(existingUser.LastName);
        result.Email.Should().Be(existingUser.Email);
        result.PhoneNumber.Should().Be(existingUser.PhoneNumber);
        result.Username.Should().Be(existingUser.Username);
    }
    [Fact]
    public async Task UpdateAsync_Should_Return_True_And_Update_User()
    {
        //Arrange
        var user = _context.Users.First();
        user.FirstName = "UpdatedFirstName";
        user.LastName = "UpdatedLastName";
        user.Email = "UpdatedEmail";
        user.PhoneNumber = "999999999";
        user.Username = "UpdatedUsername";

        //Act
        var result = await _repository.UpdateAsync(user);

        //Assert
        result.Should().BeTrue();
        var fromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        fromDb.Should().NotBeNull();
        fromDb.FirstName.Should().Be(user.FirstName);
        fromDb.LastName.Should().Be(user.LastName);
        fromDb.Email.Should().Be(user.Email);
        fromDb.PhoneNumber.Should().Be(user.PhoneNumber);
        fromDb.Username.Should().Be(user.Username);
    }
    [Fact]
    public async Task UpdateAsync_Should_Return_False_When_User_Is_Null()
    {
        var result = await _repository.UpdateAsync(null);
        result.Should().BeFalse();
    }
    [Fact]
    public async Task UpdateAsync_Should_Return_False_When_User_Is_Empty()
    {
        var result = await _repository.UpdateAsync(new User());
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task DeleteAsync_Should_Delete_Correct_User(int userId)
    {
        var existingUser = await _repository.GetAsync(userId);
        existingUser.Should().NotBeNull();
        //Arrange & Act
        var result = await _repository.DeleteAsync(userId);
        _context.ChangeTracker.Clear();
        //Assert
        result.Should().Be(true);
        _context.Users.Count().Should().BeLessThan(_usersInDatabase);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(99999999)]
    [InlineData(0)]
    public async Task DeleteAsync_Should_Return_False_When_Wrong_Id(int userId)
    {
        //Arrange & Act
        var result = await _repository.DeleteAsync(userId);

        //Assert
        result.Should().Be(false);
        _context.Users.Count().Should().Be(_usersInDatabase);
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}