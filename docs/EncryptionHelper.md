# EncryptionHelper

A utility class providing common cryptographic and encoding operations used across the ASP.NET SPA template, including random value generation, hashing, HMAC calculation, salted password hashing, checksum generation, hex encoding, sensitive data masking, and checksum verification.

## API

### `public static string GenerateRandomString(int length)`

Generates a cryptographically secure random alphanumeric string of the specified length.

- **Parameters**
  - `length` – The desired length of the random string. Must be greater than zero.
- **Return value**
  - A random string composed of uppercase letters, lowercase letters, and digits.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `length` is less than or equal to zero.

---

### `public static byte[] GenerateRandomBytes(int length)`

Generates a cryptographically secure random byte array of the specified length.

- **Parameters**
  - `length` – The number of random bytes to generate. Must be greater than zero.
- **Return value**
  - A byte array filled with cryptographically secure random values.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `length` is less than or equal to zero.

---

### `public static string ComputeSHA256Hash(string input)`

Computes the SHA-256 hash of the given input string and returns the hexadecimal representation.

- **Parameters**
  - `input` – The string to hash. Can be `null` or empty.
- **Return value**
  - The SHA-256 hash of the input as a hexadecimal string.
- **Exceptions**
  - None.

---

### `public static string ComputeHmacSha256(string key, string input)`

Computes an HMAC-SHA256 signature using the provided key and input string.

- **Parameters**
  - `key` – The secret key used for HMAC. Can be `null` or empty.
  - `input` – The data to sign. Can be `null` or empty.
- **Return value**
  - The HMAC-SHA256 signature as a hexadecimal string.
- **Exceptions**
  - None.

---

### `public static (string Hash, string Salt) GenerateSaltedHash(string input)`

Generates a salted hash suitable for password storage using PBKDF2 with HMAC-SHA256.

- **Parameters**
  - `input` – The input string to hash (e.g., a password). Can be `null` or empty.
- **Return value**
  - A tuple containing:
    - `Hash` – The derived key as a hexadecimal string.
    - `Salt` – The randomly generated salt as a hexadecimal string.
- **Exceptions**
  - None.

---

### `public static bool VerifySaltedHash(string input, string hash, string salt)`

Verifies a password against a previously generated salted hash.

- **Parameters**
  - `input` – The input string to verify (e.g., a user-provided password). Can be `null` or empty.
  - `hash` – The stored hash to compare against. Can be `null` or empty.
  - `salt` – The salt used during the original hash generation. Can be `null` or empty.
- **Return value**
  - `true` if the derived key matches the stored hash; otherwise, `false`.
- **Exceptions**
  - None.

---

### `public static string ComputeChecksum(string input)`

Computes a simple checksum of the input string using a non-cryptographic hash (FNV-1a).

- **Parameters**
  - `input` – The string to checksum. Can be `null` or empty.
- **Return value**
  - A 32-bit unsigned integer checksum formatted as an 8-character hexadecimal string.
- **Exceptions**
  - None.

---

### `public static string ToHex(byte[] bytes)`

Converts a byte array to its hexadecimal string representation.

- **Parameters**
  - `bytes` – The byte array to convert. Can be `null`.
- **Return value**
  - The hexadecimal string representation of the byte array, or `null` if the input is `null`.
- **Exceptions**
  - None.

---
### `public static string MaskSensitiveData(string input, int keepLeft, int keepRight)`

Masks sensitive data by preserving a portion of the beginning and end of the string, replacing the middle with asterisks.

- **Parameters**
  - `input` – The string to mask. Can be `null` or empty.
  - `keepLeft` – Number of characters to leave unmasked at the start. Must be non-negative.
  - `keepRight` – Number of characters to leave unmasked at the end. Must be non-negative.
- **Return value**
  - A masked string where the middle portion is replaced with asterisks, or `null` if the input is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `keepLeft` or `keepRight` is negative.
  - Throws `ArgumentException` if `keepLeft + keepRight` exceeds the length of the input.

## Usage

```csharp
// Example 1: Generating a random password and hashing it
var password = EncryptionHelper.GenerateRandomString(16);
var (hash, salt) = EncryptionHelper.GenerateSaltedHash(password);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Salt: {salt}");

// Example 2: Verifying a user-provided password
var userInput = "userPassword123!";
var isValid = EncryptionHelper.VerifySaltedHash(
    userInput,
    "a1b2c3d4e5f6...", // previously stored hash
    "f6e5d4c3b2a1..."  // previously stored salt
);
Console.WriteLine($"Password valid: {isValid}");
```

## Notes

- **Thread safety**: All methods are thread-safe and may be called concurrently from multiple threads.
- **Cryptographic randomness**: `GenerateRandomString` and `GenerateRandomBytes` use `RandomNumberGenerator`, which is suitable for cryptographic purposes.
- **Salted hashing**: `GenerateSaltedHash` and `VerifySaltedHash` use PBKDF2 with HMAC-SHA256 and a 128-bit salt, making brute-force attacks impractical.
- **Checksum limitations**: `ComputeChecksum` is not suitable for security purposes; it is intended for non-cryptographic integrity checks only.
- **Input handling**: Methods accept `null` or empty strings gracefully unless otherwise noted, returning appropriate default or empty results.
- **Hex encoding**: `ToHex` and `ComputeSHA256Hash` return lowercase hexadecimal strings without prefix.
