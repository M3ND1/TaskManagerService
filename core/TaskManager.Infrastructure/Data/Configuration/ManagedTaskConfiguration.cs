using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.Entities;

public class ManagedTaskConfiguration : IEntityTypeConfiguration<ManagedTask>
{
    public void Configure(EntityTypeBuilder<ManagedTask> builder)
    {
        builder.Property(t => t.IsCompleted).IsRequired();
        builder.Property(t => t.Priority).HasConversion<string>();
    }
}