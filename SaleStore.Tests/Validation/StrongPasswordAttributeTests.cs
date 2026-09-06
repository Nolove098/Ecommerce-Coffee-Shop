using SaleStore.Models.Validation;

namespace SaleStore.Tests.Validation;

public class StrongPasswordAttributeTests
{
    [Theory]
    [InlineData("Abcdef1!")]
    [InlineData("LongerSynthetic42!")]
    [InlineData("Abcdef1 ")]
    [InlineData("Abcdef١!")]
    public void IsValid_WhenCurrentPolicyIsSatisfied_ReturnsTrue(string password)
    {
        // Whitespace counts as special; \d also permits Unicode decimal digits.
        Assert.True(new StrongPasswordAttribute().IsValid(password));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(12345678)]
    [InlineData("")]
    [InlineData("Abcde1!")]
    [InlineData("abcdef1!")]
    [InlineData("ABCDEF1!")]
    [InlineData("Abcdefg!")]
    [InlineData("Abcdef12")]
    public void IsValid_WhenTypeLengthOrRequiredCharacterIsMissing_ReturnsFalse(object? password)
    {
        Assert.False(new StrongPasswordAttribute().IsValid(password));
    }
}
