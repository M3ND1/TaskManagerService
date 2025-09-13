using Shouldly;
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
        user.Id.ShouldBe(0);
        user.FirstName.ShouldBeNull();
        user.LastName.ShouldBeNull();
        user.Email.ShouldBeNull();
        user.PhoneNumber.ShouldBeNull();
        user.Username.ShouldBeNull();
        user.PasswordHash.ShouldBeNull();
        user.PasswordSalt.ShouldBeNull();
        user.IsActive.ShouldBeTrue();
        user.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
        user.LastLoginAt.ShouldBeNull();
        user.UpdatedAt.ShouldBeNull();
        user.AssignedTasks.ShouldBeNull();
        user.CreatedTasks.ShouldBeNull();
    }
}
