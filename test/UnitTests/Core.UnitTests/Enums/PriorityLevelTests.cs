using FluentAssertions;
using TaskManager.Core.Enums;

namespace Core.UnitTests.Enums;

public class PriorityLevelTests
{
    [Fact]
    public void PriorityLevel_ShouldHaveExpectedValues()
    {
        ((int)PriorityLevel.Low).Should().Be(0);
        ((int)PriorityLevel.Medium).Should().Be(1);
        ((int)PriorityLevel.High).Should().Be(2);

        Enum.GetValues<PriorityLevel>().Length.Should().Be(3);
    }
}
