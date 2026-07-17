# ASP.NET Core SPA Template

A production-ready ASP.NET Core template with integrated React SPA, authentication, and modern tooling.

## DateTimeExtensions

Provides a comprehensive set of extension methods for common DateTime operations including date boundary calculations, age calculation, ISO 8601 formatting, and human-readable relative time formatting. These utilities centralize timezone handling and formatting logic to ensure consistency across the application.




**Usage Example:**

```csharp
// Calculate report date boundaries for filtering
var now = DateTime.UtcNow;
var reportStart = now.StartOfDay(); // 2025-07-19 00:00:00
var reportEnd = now.EndOfDay(); // 2025-07-19 23:59:59

// Calculate weekly report boundaries (Monday to Sunday)
var weekStart = now.StartOfWeek(); // 2025-07-14 00:00:00 (Monday)
var weekEnd = now.EndOfWeek(); // 2025-07-20 23:59:59 (Sunday)

// Calculate monthly report boundaries
var monthStart = now.StartOfMonth(); // 2025-07-01 00:00:00
var monthEnd = now.EndOfMonth(); // 2025-07-31 23:59:59

// Calculate user age from birth date
var birthDate = new DateTime(1990, 5, 15);
int age = birthDate.GetAge(); // 35 (as of 2025)

// Format dates for API responses
var utcDate = DateTime.UtcNow;
string isoDate = utcDate.ToIso8601(); // "2025-07-19T14:30:45Z"

// Display user-friendly timestamps
var eventTime = DateTime.UtcNow.AddHours(-2);
string relativeTime = eventTime.ToRelativeTime(); // "2h ago"

// Check if within business hours for scheduling
if (now.IsBusinessHours())
{
    Console.WriteLine("Processing during business hours");
}

// Check if a date is in the past or future
var pastDate = DateTime.UtcNow.AddDays(-1);
var futureDate = DateTime.UtcNow.AddDays(1);

if (pastDate.IsPast()) Console.WriteLine("This date is in the past");
if (futureDate.IsFuture()) Console.WriteLine("This date is in the future");
```

## RequestContextHelper

Provides extension methods for accessing and managing request context information including correlation IDs, user details, and request metadata. This utility centralizes request context handling to ensure consistency across middleware, controllers, and services.



### Features
- **Correlation Tracking**: Generates and retrieves correlation IDs for request tracing
- **User Identification**: Extracts authenticated user IDs from HTTP context
- **Request Metadata**: Accesses client IP addresses, user agents, referrers, and headers
- **Request Analysis**: Determines if requests are AJAX, mobile, or JSON-preferring
- **Context Creation**: Builds portable `RequestContext` objects for cross-service communication

### Usage Example

```csharp
// In a controller or service
public IActionResult GetUserData(HttpContext context)
{
    // Get correlation ID for request tracing
    string correlationId = context.GetCorrelationId();
    
    // Get authenticated user ID (returns 0 if not authenticated)
    int userId = context.GetUserId();
    
    // Get client IP address (handles proxied requests via X-Forwarded-For)
    string clientIp = context.GetClientIpAddress();
    
    // Get user agent
    string userAgent = context.Request.GetUserAgent();
    
    // Check if request is AJAX
    bool isAjax = context.Request.IsAjaxRequest();
    
    // Check if request is from mobile device
    bool isMobile = context.Request.IsMobileRequest();
    
    // Get API token from Authorization header
    string? apiToken = context.Request.GetApiToken();
    
    // Create portable request context
    var requestContext = context.CreateContext();
    
    Console.WriteLine($"Processing request: {requestContext}");
    
    // Access context properties
    Console.WriteLine($"Correlation ID: {requestContext.CorrelationId}");
    Console.WriteLine($"User ID: {requestContext.UserId}");
    Console.WriteLine($"Client IP: {requestContext.ClientIp}");
    Console.WriteLine($"Path: {requestContext.Path}");
    Console.WriteLine($"Timestamp: {requestContext.Timestamp:yyyy-MM-dd HH:mm:ss}");
    
    return Ok(new { 
        CorrelationId = correlationId,
        UserId = userId,
        ClientIp = clientIp,
        IsMobile = isMobile,
        IsAjax = isAjax
    });
}

// Get query parameter safely
string? searchTerm = context.Request.GetQueryParameter("q");

// Get header value safely
string? customHeader = context.Request.GetHeaderValue("X-Custom-Header");

// Check if client wants JSON response
bool wantsJson = context.Request.WantsJson();
```

## EncryptionHelper

Provides secure cryptographic operations including random string generation, hashing, HMAC signatures, and data masking. Uses modern algorithms (SHA256, HMAC, PBKDF2) for data protection and integrity verification. These utilities are essential for password storage, API authentication, and sensitive data handling.

### Usage Example

```csharp
// Generate secure random tokens for API keys and session management
string apiKey = EncryptionHelper.GenerateRandomString(64);
byte[] secureBytes = EncryptionHelper.GenerateRandomBytes(32);

// Compute hashes for data integrity and password storage
string passwordHash = EncryptionHelper.ComputeSHA256Hash("userPassword123");
string hmacSignature = EncryptionHelper.ComputeHmacSha256(
    "messageData", 
    "secretKey123"
);

// Secure password hashing with salt
var saltedHash = EncryptionHelper.GenerateSaltedHash("userPassword");
bool isValid = EncryptionHelper.VerifySaltedHash(
    "userPassword",
    saltedHash.Hash,
    saltedHash.Salt
);

// Generate checksums for data tampering detection
string fileChecksum = EncryptionHelper.ComputeChecksum(File.ReadAllBytes("data.bin"));

// Convert data to hexadecimal for logging
string hexValue = EncryptionHelper.ToHex("sensitiveData");

// Mask sensitive data in logs
string maskedCard = EncryptionHelper.MaskSensitiveData("4532123456789012", 4); // "4532****9012"
string maskedEmail = EncryptionHelper.MaskSensitiveData("user@example.com", 3); // "use****@example.com"
```

## ErrorMappingHelper

Provides a centralized mechanism for mapping exceptions and error conditions to standardized HTTP status codes, error codes, user-friendly messages, and logging levels. This utility ensures consistent error handling across API controllers, services, and middleware by transforming raw exceptions into structured error responses that are both machine-readable and user-friendly.

### Usage Example

```csharp
// In a controller action
public IActionResult GetUser(int userId)
{
    try
    {
        var user = _userService.GetUser(userId);
        if (user == null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }
    catch (Exception ex)
    {
        // Map exception to standardized error response
        var errorDetails = ErrorMappingHelper.ExtractErrorDetails(ex);
        
        // Get HTTP status code for the exception type
        int statusCode = ErrorMappingHelper.MapToStatusCode(ex);
        
        // Get error code for client-side handling
        string errorCode = ErrorMappingHelper.MapToErrorCode(ex);
        
        // Get user-friendly message
        string userMessage = ErrorMappingHelper.MapToUserMessage(ex);
        
        // Get appropriate log level
        var logLevel = ErrorMappingHelper.GetLogLevel(ex);
        
        // Log the error appropriately
        switch (logLevel)
        {
            case LogLevel.Error:
                _logger.LogError(ex, "Error processing user request: {ErrorCode} - {UserMessage}", errorCode, userMessage);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(ex, "Warning: {ErrorCode} - {UserMessage}", errorCode, userMessage);
                break;
            case LogLevel.Information:
                _logger.LogInformation(ex, "Information: {ErrorCode} - {UserMessage}", errorCode, userMessage);
                break;
        }
        
        // Check if error is transient/retryable
        var retryInfo = ErrorMappingHelper.GetRetryInfo(ex);
        if (retryInfo.Retryable)
        {
            _logger.LogWarning("Transient error occurred. Retry after {RetryAfterSeconds} seconds", retryInfo.RetryAfterSeconds);
        }
        
        // Return standardized error response
        return StatusCode(statusCode, new
        {
            ErrorCode = errorCode,
            Message = userMessage,
            Details = errorDetails.ToString(),
            Timestamp = DateTime.UtcNow
        });
    }
}

// Example usage in a service
public async Task<User> GetUserAsync(int userId)
{
    try
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }
        
        return user;
    }
    catch (SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
    {
        throw new InvalidOperationException("Database table not found", sqlEx);
    }
    catch (SqlException sqlEx) when (ErrorMappingHelper.IsTransientError(sqlEx))
    {
        throw new InvalidOperationException("Database operation failed due to transient error", sqlEx);
    }
    catch (Exception ex)
    {
        if (ErrorMappingHelper.IsCriticalError(ex))
        {
            _logger.LogCritical(ex, "Critical error in user service");
        }
        
        throw;
    }
}

// Check if specific error types are transient
if (ErrorMappingHelper.IsTransientError(sqlException))
{
    // Implement retry logic
    await RetryPolicy.ExecuteAsync(async () => await RetryOperation());
}

// Check if error is critical
if (ErrorMappingHelper.IsCriticalError(exception))
{
    // Trigger incident response procedures
    _alertService.TriggerCriticalAlert(exception);
}
```

## JsonSerializationHelper

Provides consistent JSON serialization and deserialization utilities with standardized options across the application. Centralizes JSON configuration including camelCase naming policy, null value handling, and enum converter settings to ensure uniformity in API responses, logging, and data persistence.

### Usage Example

```csharp
// Define a sample model
public class UserProfile
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum UserRole
{
    Admin,
    User,
    Guest
}

// Serialize object to JSON (compact format for API responses)
var user = new UserProfile
{
    Id = 1,
    Username = "johndoe",
    Email = "john@example.com",
    Role = UserRole.Admin,
    CreatedAt = DateTime.UtcNow
};

string json = JsonSerializationHelper.Serialize(user);
// Output: {"id":1,"username":"johndoe","email":"john@example.com","role":"admin","createdAt":"2025-07-19T14:30:45.123Z"}



// Serialize with pretty formatting (for logging/debugging)
string prettyJson = JsonSerializationHelper.SerializePretty(user);
// Output: formatted JSON with indentation

// Deserialize JSON to typed object
string apiResponse = "{\\"id\\":2,\\"username\\":\\"janedoe\\",\\"email\\":\\"jane@example.com\\",\\"role\\":\\"user\\",\\"createdAt\\":\\"2025-07-18T10:15:30Z\\"}";
var deserializedUser = JsonSerializationHelper.Deserialize<UserProfile>(apiResponse);

// Safe deserialization (returns null on failure)
string invalidJson = "{invalid json}";
var safeResult = JsonSerializationHelper.DeserializeSafe<UserProfile>(invalidJson); // Returns null

// Validate JSON without deserialization
bool isValid = JsonSerializationHelper.IsValidJson(json); // true
bool isInvalid = JsonSerializationHelper.IsValidJson("{invalid"); // false

// Get default serializer options for custom scenarios
var options = JsonSerializationHelper.GetDefaultOptions();

// Get pretty formatting options
var prettyOptions = JsonSerializationHelper.GetPrettyOptions();

// Parse JSON to JsonElement for flexible querying
var jsonElement = JsonSerializationHelper.ParseJsonElement(json);
if (jsonElement.HasValue)
{
    var root = jsonElement.Value;
    var username = root.GetProperty("username").GetString();
}

// Convert between object types via JSON
var userData = new { user.Id, user.Username, user.Role };
var convertedUser = JsonSerializationHelper.ConvertObject<UserProfile>(userData);

// Deserialize from stream (useful for HTTP request bodies)
using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
var streamUser = await JsonSerializationHelper.DeserializeAsync<UserProfile>(stream);
```

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---


**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**


[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
