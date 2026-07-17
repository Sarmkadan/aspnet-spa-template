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

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---


**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**


[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
