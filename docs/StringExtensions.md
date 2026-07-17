# StringExtensions
The `StringExtensions` class provides a set of static methods for manipulating and validating strings. It offers various utility functions for tasks such as sanitizing input, generating slugs, truncating text, and checking email addresses. These methods can be used throughout an application to ensure consistent and safe handling of string data.

## API
* `public static string Sanitize(string input)`: Sanitizes the input string by removing or escaping special characters. Returns the sanitized string.
* `public static string ToSlug(string input)`: Converts the input string into a slug by replacing spaces with hyphens and removing special characters. Returns the slug string.
* `public static string Truncate(string input, int length)`: Truncates the input string to the specified length. Returns the truncated string.
* `public static string ToDisplayName(string input)`: Converts the input string into a display name by capitalizing the first letter and removing special characters. Returns the display name string.
* `public static bool IsValidEmail(string email)`: Checks if the input string is a valid email address. Returns `true` if the email is valid, `false` otherwise.
* `public static bool IsAlphaNumeric(string input)`: Checks if the input string contains only alphanumeric characters. Returns `true` if the string is alphanumeric, `false` otherwise.
* `public static string OrIfEmpty(string input, string defaultValue)`: Returns the input string if it is not empty, otherwise returns the default value. Returns the input string or the default value.
* `public static string HtmlEncode(string input)`: Encodes the input string for use in HTML by replacing special characters with their corresponding HTML entities. Returns the encoded string.

## Usage
```csharp
// Example 1: Sanitizing user input
string userInput = "Hello, World!";
string sanitizedInput = StringExtensions.Sanitize(userInput);
Console.WriteLine(sanitizedInput); // Output: "Hello World"

// Example 2: Validating an email address
string email = "john.doe@example.com";
bool isValid = StringExtensions.IsValidEmail(email);
Console.WriteLine(isValid); // Output: True
```

## Notes
The `StringExtensions` class is designed to be thread-safe, as all methods are static and do not rely on any shared state. However, it is worth noting that some methods may throw exceptions if the input is null or empty, so it is recommended to check for these conditions before calling the methods. Additionally, the `Sanitize` and `HtmlEncode` methods may not cover all possible edge cases, and it is recommended to use them in conjunction with other validation and encoding mechanisms to ensure the security and integrity of the application.
