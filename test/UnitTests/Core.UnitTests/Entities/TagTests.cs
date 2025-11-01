using FluentAssertions;
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
        tag.Id.Should().Be(0);
        tag.Name.Should().Be(string.Empty);
        tag.Color.Should().BeNull();
        tag.Description.Should().BeNull();
        tag.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        tag.UpdatedAt.Should().BeNull();
        tag.CreatedById.Should().BeNull();
        tag.CreatedBy.Should().BeNull();
        tag.ManagedTasks.Should().BeNull();
    }
}
