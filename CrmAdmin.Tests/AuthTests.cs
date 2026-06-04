using Xunit;
using Moq;
using CrmAdmin.Web.Interfaces; // Adjust namespace if your interfaces are elsewhere
using CrmAdmin.Web.Models;

namespace CrmAdmin.Tests;

public class AuthLogicTests
{
    [Fact]
    public void Login_WithValidCredentials_ReturnsUser()
    {
        // ARRANGE
        // We create a "Fake" version of your User Provider
        var mockProvider = new Mock<IUserProvider>();
        
        // We tell the fake provider: "If someone asks for 'Lee', return a valid User object"
        mockProvider.Setup(p => p.ValidateUser("Lee", "Password123"))
                    .Returns(new UserRecord { Username = "Lee", FullName = "Lee Wraith" });

        // ACT
        // We pass the "Fake" provider into your logic
        var result = mockProvider.Object.ValidateUser("Lee", "Password123");

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("Lee Wraith", result.FullName);
    }
}