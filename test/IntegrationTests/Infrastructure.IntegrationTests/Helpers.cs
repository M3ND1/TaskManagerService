using TaskManager.Core.Entities;
using TaskManager.Core.Enums;

public class Helpers
{
    public static ManagedTask[] CreateManagedTasks()
    {
        return
        [
            new() {
                Id = 1,
                Title = "Test",
                Description = "Description",
                CreatedAt = new DateTime(),
                Priority = PriorityLevel.Low,
                IsCompleted = false,
                CreatedById = 1,
            },
            new() {
                Id = 2,
                Title = "Testaaa",
                Description = "Description2",
                CreatedAt = new DateTime(),
                Priority = PriorityLevel.High,
                IsCompleted = false,
                CreatedById = 1,
            },
            new() {
                Id = 3,
                Title = "Tessttt3",
                Description = "Description3",
                CreatedAt = new DateTime(),
                Priority = PriorityLevel.Medium,
                IsCompleted = false,
                CreatedById = 1,
            },
            new() {
                Id = 4,
                Title = "Test4",
                Description = "Description4",
                CreatedAt = new DateTime(),
                Priority = PriorityLevel.Low,
                IsCompleted = true,
                CreatedById = 1,
            },
        ];
    }
    public static User[] CreateUsers()
    {
        return
        [
            new() { Id = 1, FirstName = "Chris", LastName = "Nynek", Email="Chris123@gmail.com", PhoneNumber = "123456789", Username = "ChrisNyn", PasswordHash= Guid.NewGuid().ToString(), PasswordSalt=Guid.NewGuid().ToString(), IsActive = true, CreatedAt = new DateTime() },
            new() { Id = 2, FirstName = "John", LastName = "Doe", Email="JohnDoe@gmail.com", PhoneNumber = "987654321", Username = "JohnDoe", PasswordHash= Guid.NewGuid().ToString(), PasswordSalt=Guid.NewGuid().ToString(), IsActive = true, CreatedAt = new DateTime() },
            new() { Id = 3, FirstName = "Jane", LastName = "Smith", Email="JaneSmith@gmail.com", PhoneNumber = "456789123", Username = "JaneSmith", PasswordHash= Guid.NewGuid().ToString(), PasswordSalt=Guid.NewGuid().ToString(), IsActive = true, CreatedAt = new DateTime() },
        ];
    }
}