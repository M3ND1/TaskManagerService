using TaskManager.Application;
using TaskManager.Core.Entities;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeletAsync(User user);
}