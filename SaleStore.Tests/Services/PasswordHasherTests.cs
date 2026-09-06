using SaleStore.Services;

namespace SaleStore.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Theory]
    [InlineData("Synthetic!Coffee42")]
    [InlineData("Cà phê ☕ synthetic 42!")]
    [InlineData("")]
    public void Verify_WhenPasswordMatches_ReturnsTrue(string password)
    {
        // Password policy is enforced by validation, independently of hashing.
        var stored = _hasher.CreateHash(password);

        var verified = _hasher.Verify(password, stored.Hash, stored.Salt);

        Assert.True(verified);
    }

    [Theory]
    [InlineData("synthetic!Coffee42")]
    [InlineData("Synthetic!Coffee42 ")]
    [InlineData("")]
    public void Verify_WhenPasswordDiffers_ReturnsFalse(string candidate)
    {
        var stored = _hasher.CreateHash("Synthetic!Coffee42");

        Assert.False(_hasher.Verify(candidate, stored.Hash, stored.Salt));
    }

    [Fact]
    public void Verify_WhenStoredHashIsTampered_ReturnsFalse()
    {
        var stored = _hasher.CreateHash("Synthetic!Coffee42");
        var bytes = Convert.FromBase64String(stored.Hash);
        bytes[0] ^= 1;

        Assert.False(_hasher.Verify("Synthetic!Coffee42", Convert.ToBase64String(bytes), stored.Salt));
    }

    [Theory]
    [InlineData("not-base64!", "AA==")]
    [InlineData("AA==", "not-base64!")]
    public void Verify_WhenStoredEncodingIsMalformed_ThrowsFormatException(string hash, string salt)
    {
        // Characterizes the current contract; this helper does not swallow corrupt storage.
        Assert.Throws<FormatException>(() => _hasher.Verify("synthetic", hash, salt));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AA==")]
    public void Verify_WhenStoredHashLengthIsInvalid_ReturnsFalse(string hash)
    {
        var stored = _hasher.CreateHash("Synthetic!Coffee42");

        Assert.False(_hasher.Verify("Synthetic!Coffee42", hash, stored.Salt));
    }
}
