#nullable enable
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="EncryptionHelper"/> class.
/// Tests various encryption, hashing, and cryptographic operations including random generation,
/// hashing, HMAC, PBKDF2, checksums, hex conversion, and data masking.
/// </summary>
public sealed class EncryptionHelperUnitTests
{
    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateRandomString"/> generates a string of specified length.
    /// </summary>
    [Fact]
    public void GenerateRandomString_WithSpecifiedLength_ReturnsStringOfCorrectLength()
    {
        // Act
        var result = EncryptionHelper.GenerateRandomString(16);

        // Assert
        result.Should().HaveLength(16);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateRandomString"/> uses default length when not specified.
    /// </>
    [Fact]
    public void GenerateRandomString_WithoutLengthParameter_UsesDefaultLength()
    {
        // Act
        var result = EncryptionHelper.GenerateRandomString();

        // Assert
        result.Should().HaveLength(32); // Default length from source
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateRandomString"/> generates different values on subsequent calls.
    /// </summary>
    [Fact]
    public void GenerateRandomString_WithSameLength_GeneratesDifferentValues()
    {
        // Act
        var result1 = EncryptionHelper.GenerateRandomString(32);
        var result2 = EncryptionHelper.GenerateRandomString(32);

        // Assert
        result1.Should().NotBe(result2);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateRandomBytes"/> generates byte array of specified length.
    /// </summary>
    [Fact]
    public void GenerateRandomBytes_WithSpecifiedLength_ReturnsArrayOfCorrectLength()
    {
        // Act
        var result = EncryptionHelper.GenerateRandomBytes(16);

        // Assert
        result.Should().HaveCount(16);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeSHA256Hash"/> returns empty string for null input.
    /// </summary>
    [Fact]
    public void ComputeSHA256Hash_WithNullInput_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ComputeSHA256Hash(null!);

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeSHA256Hash"/> returns empty string for empty input.
    /// </summary>
    [Fact]
    public void ComputeSHA256Hash_WithEmptyInput_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ComputeSHA256Hash(string.Empty);

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeSHA256Hash"/> produces consistent hash for same input.
    /// </summary>
    [Fact]
    public void ComputeSHA256Hash_WithSameInput_ReturnsConsistentHash()
    {
        // Arrange
        const string input = "test password";

        // Act
        var hash1 = EncryptionHelper.ComputeSHA256Hash(input);
        var hash2 = EncryptionHelper.ComputeSHA256Hash(input);

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeSHA256Hash"/> produces different hashes for different inputs.
    /// </summary>
    [Fact]
    public void ComputeSHA256Hash_WithDifferentInputs_ReturnsDifferentHashes()
    {
        // Act
        var hash1 = EncryptionHelper.ComputeSHA256Hash("password1");
        var hash2 = EncryptionHelper.ComputeSHA256Hash("password2");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeHmacSha256"/> returns empty string for null data.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_WithNullData_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ComputeHmacSha256(null!, "key");

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeHmacSha256"/> returns empty string for null key.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_WithNullKey_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ComputeHmacSha256("data", null!);

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeHmacSha256"/> returns empty string for empty data.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_WithEmptyData_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ComputeHmacSha256(string.Empty, "key");

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeHmacSha256"/> returns empty string for empty key.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_WithEmptyKey_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ComputeHmacSha256("data", string.Empty);

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeHmacSha256"/> produces consistent HMAC for same inputs.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_WithSameInputs_ReturnsConsistentHmac()
    {
        // Arrange
        const string data = "test data";
        const string key = "secret key";

        // Act
        var hmac1 = EncryptionHelper.ComputeHmacSha256(data, key);
        var hmac2 = EncryptionHelper.ComputeHmacSha256(data, key);

        // Assert
        hmac1.Should().Be(hmac2);
        hmac1.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeHmacSha256"/> produces different HMACs for different keys.
    /// </summary>
    [Fact]
    public void ComputeHmacSha256_WithSameDataDifferentKeys_ReturnsDifferentHmacs()
    {
        // Arrange
        const string data = "test data";

        // Act
        var hmac1 = EncryptionHelper.ComputeHmacSha256(data, "key1");
        var hmac2 = EncryptionHelper.ComputeHmacSha256(data, "key2");

        // Assert
        hmac1.Should().NotBe(hmac2);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateSaltedHash"/> returns empty strings for null input.
    /// </summary>
    [Fact]
    public void GenerateSaltedHash_WithNullInput_ReturnsEmptyStrings()
    {
        // Act
        var (hash, salt) = EncryptionHelper.GenerateSaltedHash(null!);

        // Assert
        hash.Should().Be(string.Empty);
        salt.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateSaltedHash"/> returns empty strings for empty input.
    /// </summary>
    [Fact]
    public void GenerateSaltedHash_WithEmptyInput_ReturnsEmptyStrings()
    {
        // Act
        var (hash, salt) = EncryptionHelper.GenerateSaltedHash(string.Empty);

        // Assert
        hash.Should().Be(string.Empty);
        salt.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.GenerateSaltedHash"/> generates different salts on subsequent calls.
    /// </summary>
    [Fact]
    public void GenerateSaltedHash_WithSameInput_GeneratesDifferentSalts()
    {
        // Act
        var (hash1, salt1) = EncryptionHelper.GenerateSaltedHash("password");
        var (hash2, salt2) = EncryptionHelper.GenerateSaltedHash("password");

        // Assert
        salt1.Should().NotBe(salt2);
        // Hashes should also be different due to different salts
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.VerifySaltedHash"/> correctly verifies a valid password.
    /// </summary>
    [Fact]
    public void VerifySaltedHash_WithValidPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "mySecurePassword123!";
        var (hash, salt) = EncryptionHelper.GenerateSaltedHash(password);

        // Act
        var result = EncryptionHelper.VerifySaltedHash(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.VerifySaltedHash"/> returns false for incorrect password.
    /// </summary>
    [Fact]
    public void VerifySaltedHash_WithInvalidPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "mySecurePassword123!";
        const string wrongPassword = "wrongPassword";
        var (hash, salt) = EncryptionHelper.GenerateSaltedHash(password);

        // Act
        var result = EncryptionHelper.VerifySaltedHash(wrongPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.VerifySaltedHash"/> returns false for null inputs.
    /// </summary>
    [Fact]
    public void VerifySaltedHash_WithNullInputs_ReturnsFalse()
    {
        // Act
        var result = EncryptionHelper.VerifySaltedHash(null!, null!, null!);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeChecksum"/> returns correct checksum for byte array.
    /// </summary>
    [Fact]
    public void ComputeChecksum_WithByteArray_ReturnsEightCharacterHexString()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        // Act
        var result = EncryptionHelper.ComputeChecksum(data);

        // Assert
        result.Should().HaveLength(8);
        result.Should().MatchRegex("[0-9a-f]{8}");
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeChecksum"/> throws ArgumentNullException for null input.
    /// </summary>
    [Fact]
    public void ComputeChecksum_WithNullInput_ThrowsArgumentNullException()
    {
        // Act
        var act = () => EncryptionHelper.ComputeChecksum(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ComputeChecksum"/> returns checksum for empty array.
    /// </summary>
    [Fact]
    public void ComputeChecksum_WithEmptyArray_ReturnsChecksum()
    {
        // Act
        var result = EncryptionHelper.ComputeChecksum(Array.Empty<byte>());

        // Assert
        result.Should().HaveLength(8);
        result.Should().MatchRegex("[0-9a-f]{8}");
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ToHex"/> returns empty string for null input.
    /// </summary>
    [Fact]
    public void ToHex_WithNullInput_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ToHex(null!);

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ToHex"/> returns empty string for empty input.
    /// </summary>
    [Fact]
    public void ToHex_WithEmptyInput_ReturnsEmptyString()
    {
        // Act
        var result = EncryptionHelper.ToHex(string.Empty);

        // Assert
        result.Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.ToHex"/> correctly converts string to hexadecimal.
    /// </summary>
    [Fact]
    public void ToHex_WithValidInput_ReturnsCorrectHexRepresentation()
    {
        // Arrange
        const string input = "Hello";

        // Act
        var result = EncryptionHelper.ToHex(input);

        // Assert
        result.Should().Be("48656c6c6f"); // Hex for "Hello"
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.MaskSensitiveData"/> masks data correctly.
    /// </summary>
    [Fact]
    public void MaskSensitiveData_WithValidInput_ReturnsMaskedString()
    {
        // Arrange
        const string data = "1234567890123456"; // 16 digits

        // Act
        var result = EncryptionHelper.MaskSensitiveData(data, 4);

        // Assert
        result.Should().Be("1234********3456");
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.MaskSensitiveData"/> handles short strings.
    /// </summary>
    [Fact]
    public void MaskSensitiveData_WithShortInput_ReturnsAllMasked()
    {
        // Arrange
        const string data = "1234"; // Exactly visibleChars * 2

        // Act
        var result = EncryptionHelper.MaskSensitiveData(data, 2);

        // Assert
        result.Should().Be("****");
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.MaskSensitiveData"/> handles null input.
    /// </summary>
    [Fact]
    public void MaskSensitiveData_WithNullInput_ReturnsAllMasked()
    {
        // Act
        var result = EncryptionHelper.MaskSensitiveData(null!, 4);

        // Assert
        result.Should().Be("****");
    }

    /// <summary>
    /// Tests that <see cref="EncryptionHelper.MaskSensitiveData"/> handles empty input.
    /// </summary>
    [Fact]
    public void MaskSensitiveData_WithEmptyInput_ReturnsAllMasked()
    {
        // Act
        var result = EncryptionHelper.MaskSensitiveData(string.Empty, 4);

        // Assert
        result.Should().Be("****");
    }
}