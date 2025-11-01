using FluentAssertions;
using TaskManager.Core.Entities;

namespace Core.UnitTests.Entities;

public class ManagedTaskTests
{
    [Fact]
    public void NewManagedTask_ShouldHaveDefaultValues()
    {
        // Arrange
        var task = new ManagedTask();

        // Assert
        task.Id.Should().Be(0);
        task.Title.Should().Be(string.Empty);
        task.Description.Should().Be(string.Empty);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        task.UpdatedAt.Should().BeNull();
        task.DueDate.Should().BeNull();
        task.CompletedAt.Should().BeNull();
        task.Priority.Should().Be(TaskManager.Core.Enums.PriorityLevel.Medium);
        task.IsCompleted.Should().BeFalse();
        task.EstimatedHours.Should().BeNull();
        task.ActualHours.Should().BeNull();
        task.AssignedToId.Should().BeNull();
        task.CreatedById.Should().Be(0);
        task.AssignedTo.Should().BeNull();
        task.CreatedBy.Should().BeNull();
        task.Tags.Should().BeNull();
    }
}
