using Shouldly;
using TaskManager.Core.Entities;

namespace Core.UnitTests.Entities;

public class TagTests
{
    [Fact]
    public void NewTag_ShouldHaveDefaultValues()
    {
        // Arrange
        var tag = new Tag();

        // Assert
        tag.Id.ShouldBe(0);
        tag.Name.ShouldBe(string.Empty);
        tag.Color.ShouldBeNull();
        tag.Description.ShouldBeNull();
        tag.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
        tag.UpdatedAt.ShouldBeNull();
        tag.CreatedById.ShouldBeNull();
        tag.CreatedBy.ShouldBeNull();
        tag.ManagedTasks.ShouldBeNull();
    }
}
