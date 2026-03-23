using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data.Database;

namespace TaskManager.Infrastructure.Repositories
{
    public class TagRepository(TaskManagerDbContext dbContext) : ITagRepository
    {
        private readonly TaskManagerDbContext _dbContext = dbContext;

        public async Task<bool> AddAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            await _dbContext.Tags.AddAsync(tag, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Tag?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Tags
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            if (tag == null) return false;

            try
            {
                _dbContext.Tags.Update(tag);
                var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
                return affectedRows > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            Tag? searchedTag = await GetAsync(id, cancellationToken);

            if (searchedTag == null)
                return false;
            _dbContext.Tags.Remove(searchedTag);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
