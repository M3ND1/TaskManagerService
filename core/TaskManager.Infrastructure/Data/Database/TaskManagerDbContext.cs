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
            e.Property(e => e.LastName).IsRequired().HasMaxLength(150);
            e.Property(e => e.Email).IsRequired().HasMaxLength(100);
            e.Property(e => e.PhoneNumber).HasMaxLength(12);
            e.Property(e => e.Username).IsRequired().HasMaxLength(50);
            e.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            e.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(256);
            e.Property(e => e.CreatedAt).IsRequired();
            e.Property(e => e.LastLoginAt).IsRequired(false);
            e.Property(e => e.UpdatedAt).IsRequired(false);
            e.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Tag>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).IsRequired(true);
            e.Property(e => e.Color).IsRequired(false);
            e.Property(e => e.Description).IsRequired(false).HasMaxLength(150);
            e.Property(e => e.CreatedAt).IsRequired(true);
            e.Property(e => e.UpdatedAt).IsRequired(false);

            e.HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(t => t.CreatedById);
            e.HasIndex(t => t.Name);
        });

        modelBuilder.Entity<ManagedTask>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.Title).IsRequired().HasMaxLength(200);
            e.Property(e => e.Description).IsRequired().HasMaxLength(500);
            e.Property(e => e.CreatedAt).IsRequired();
            e.Property(e => e.UpdatedAt).IsRequired(false);
            e.Property(e => e.DueDate).IsRequired(false);
            e.Property(e => e.CompletedAt).IsRequired(false);
            e.Property(e => e.Priority).IsRequired();
            e.Property(e => e.IsCompleted).IsRequired().HasDefaultValue(false);
            e.Property(e => e.EstimatedHours).IsRequired(false);
            e.Property(e => e.ActualHours).IsRequired(false);
            e.Property(e => e.Priority).HasConversion<string>();

            e.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedById)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(t => t.Tags)
                .WithMany(t => t.ManagedTasks)
                .UsingEntity(e => e.ToTable("TaskTags"));

            e.HasIndex(t => t.DueDate);
            e.HasIndex(t => t.Priority);
            e.HasIndex(t => t.IsCompleted);
            e.HasIndex(t => t.AssignedToId);
            e.HasIndex(t => t.CreatedById);
            e.HasIndex(t => new { t.AssignedToId, t.IsCompleted });
        });
    }
}