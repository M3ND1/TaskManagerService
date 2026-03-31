using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces;

public interface ITagRepository
{
    Task<bool> AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task<Tag?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

