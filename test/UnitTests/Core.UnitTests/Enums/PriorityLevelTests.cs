using Shouldly;
using TaskManager.Core.Enums;

namespace Core.UnitTests.Enums;

public class PriorityLevelTests
{
    [Fact]
    public void PriorityLevel_ShouldHaveExpectedValues()
    {
        ((int)PriorityLevel.Low).ShouldBe(0);
        ((int)PriorityLevel.Medium).ShouldBe(1);
        ((int)PriorityLevel.High).ShouldBe(2);

        Enum.GetValues<PriorityLevel>().Length.ShouldBe(3);
    }
}
