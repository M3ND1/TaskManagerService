using Shouldly;
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
        task.Id.ShouldBe(0);
        task.Title.ShouldBe(string.Empty);
        task.Description.ShouldBe(string.Empty);
        task.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
        task.UpdatedAt.ShouldBeNull();
        task.DueDate.ShouldBeNull();
        task.CompletedAt.ShouldBeNull();
        task.Priority.ShouldBe(TaskManager.Core.Enums.PriorityLevel.Medium);
        task.IsCompleted.ShouldBeFalse();
        task.EstimatedHours.ShouldBeNull();
        task.ActualHours.ShouldBeNull();
        task.AssignedToId.ShouldBeNull();
        task.CreatedById.ShouldBe(0);
        task.AssignedTo.ShouldBeNull();
        task.CreatedBy.ShouldBeNull();
        task.Tags.ShouldBeNull();
    }
}
