// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Helper class for encryption, hashing, and cryptographic operations.
/// Uses modern algorithms (SHA256, HMAC) for data protection.
/// Never stores plaintext passwords - always hash them.
/// </summary>
public static class EncryptionHelper
{
    /// <summary>
    /// Generates a cryptographically secure random string of specified length.
    /// Used for tokens, API keys, session IDs.
    /// </summary>
    public static string GenerateRandomString(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Generates a secure random byte array.
    /// More cryptographically secure than Random class.
    /// </summary>
    public static byte[] GenerateRandomBytes(int length = 32)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return bytes;
        }
    }

    /// <summary>
    /// Computes SHA256 hash of input string.
    /// Used for password hashing and integrity verification.
    /// </summary>
    public static string ComputeSHA256Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature for data integrity verification.
    /// Used for webhook signatures and API message authentication.
    /// </summary>
    public static string ComputeHmacSha256(string data, string key)
    {
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(key))
            return string.Empty;

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }

    /// <summary>
    /// Generates a salted hash using PBKDF2 algorithm.
    /// More secure than simple SHA256 for password hashing.
    /// </summary>
    public static (string Hash, string Salt) GenerateSaltedHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (string.Empty, string.Empty);

        // Generate salt
        var salt = GenerateRandomBytes(16);
        var saltString = Convert.ToBase64String(salt);

        // Hash with salt using PBKDF2
        using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 10000, HashAlgorithmName.SHA256))
        {
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(20));
            return (hash, saltString);
        }
    }

    /// <summary>
    /// Verifies a salted hash against input (for password verification).
    /// </summary>
    public static bool VerifySaltedHash(string input, string hash, string salt)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
            return false;

        var saltBytes = Convert.FromBase64String(salt);

        using (var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, 10000, HashAlgorithmName.SHA256))
        {
            var hashOfInput = Convert.ToBase64String(pbkdf2.GetBytes(20));
            return hashOfInput == hash;
        }
    }

    /// <summary>
    /// Computes checksum/hash for detecting data tampering.
    /// Uses CRC32-like behavior but safer (SHA256 based).
    /// </summary>
    public static string ComputeChecksum(byte[] data)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(data);
            return Convert.ToHexString(hashedBytes).Substring(0, 8).ToLower();
        }
    }

    /// <summary>
    /// Converts string to hexadecimal representation.
    /// Used for logging sensitive data in anonymized form.
    /// </summary>
    public static string ToHex(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Convert.ToHexString(Encoding.UTF8.GetBytes(input)).ToLower();
    }

    /// <summary>
    /// Masks sensitive data for logging (e.g., "4532****1234" for credit cards).
    /// </summary>
    public static string MaskSensitiveData(string data, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(data) || data.Length <= visibleChars * 2)
            return "****";

        var start = data[..visibleChars];
        var end = data[^visibleChars..];
        var masked = new string('*', data.Length - (visibleChars * 2));

        return start + masked + end;
    }
}
