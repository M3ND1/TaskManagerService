using FluentAssertions;
using TaskManager.Core.Entities;

namespace Core.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void NewUser_ShouldHaveDefaultValues()
    {
        // Arrange
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
        user.Email.Should().BeNull();
        user.PhoneNumber.Should().BeNull();
        user.Username.Should().BeNull();
        user.PasswordHash.Should().BeNull();
        user.PasswordSalt.Should().BeNull();
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastLoginAt.Should().BeNull();
        user.UpdatedAt.Should().BeNull();
        user.AssignedTasks.Should().BeNull();
        user.CreatedTasks.Should().BeNull();
    }
}
