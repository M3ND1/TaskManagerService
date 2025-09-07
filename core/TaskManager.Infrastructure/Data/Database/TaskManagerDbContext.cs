

using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;

namespace TaskManager.Infrastructure;

public class TaskManagerDbContext : DbContext
{
    public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }
    public DbSet<ManagedTask> ManagedTasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            e.Property(e => e.Email).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        //MangagedTask

        modelBuilder.Entity<ManagedTask>()
            .HasMany(t => t.Tags)
            .WithMany(t => t.ManagedTasks)
            .UsingEntity(e => e.ToTable("TaskTags"));

        modelBuilder.Entity<ManagedTask>()
            .HasIndex(t => t.IsCompleted);

        modelBuilder.Entity<ManagedTask>()
            .HasIndex(t => t.DueDate);

        modelBuilder.Entity<ManagedTask>()
            .HasIndex(t => t.Priority);
    }
}