# ASP.NET Core SPA Template

A production-ready ASP.NET Core template with integrated React SPA, authentication, and modern tooling.

## ProductsController

Manages product catalog operations including CRUD operations, product search, category filtering, and featured product management. The controller provides RESTful endpoints for product lifecycle management with proper validation and response handling.

### Endpoints

- `GET /api/products/{id}` - Retrieve a single product by ID
- `GET /api/products` - List all products with pagination
- `GET /api/products/category/{category}` - List products by category with pagination
- `GET /api/products/featured` - Get featured products (limited set)
- `GET /api/products/top-rated` - Get top-rated products (limited set)
- `GET /api/products/search` - Search products by search term
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update an existing product
- `PATCH /api/products/{id}/availability` - Update product availability status
- `PATCH /api/products/{id}/featured` - Update product featured status
- `DELETE /api/products/{id}` - Delete a product

### Usage Example

```csharp
// Configure ProductService in Program.cs
builder.Services.AddScoped<ProductService>();

// Example: Get a single product
var productResponse = await client.GetFromJsonAsync<ProductResponse>("/api/products/1");
Console.WriteLine($"Product: {productResponse?.Name}, Price: {productResponse?.Price}");

// Example: List all products with pagination
var productsPage1 = await client.GetFromJsonAsync<ProductListResponse>("/api/products?pageNumber=1&pageSize=10");
Console.WriteLine($"Total products: {productsPage1?.TotalCount}");

// Example: Get products by category
var electronics = await client.GetFromJsonAsync<ProductListResponse>(
    "/api/products/category/electronics?pageNumber=1&pageSize=5");

// Example: Get featured products
var featured = await client.GetFromJsonAsync<List<ProductResponse>>("/api/products/featured?limit=5");

// Example: Search products
var searchResults = await client.GetFromJsonAsync<List<ProductResponse>>(
    "/api/products/search?searchTerm=laptop");

// Example: Create a new product
var newProduct = new CreateProductRequest
{
    Name = "Premium Wireless Headphones",
    Description = "Noise-cancelling wireless headphones with 30-hour battery",
    Price = 299.99m,
    Category = ProductCategory.Electronics,
    StockQuantity = 50,
    IsFeatured = true,
    IsAvailable = true
};
var createdProduct = await client.PostAsJsonAsync<ProductResponse>("/api/products", newProduct);

// Example: Update a product
var updateRequest = new UpdateProductRequest
{
    Name = "Premium Wireless Headphones (Updated)",
    Description = "Noise-cancelling wireless headphones with 30-hour battery - Updated model",
    Price = 279.99m,
    StockQuantity = 45
};
var updatedProduct = await client.PutAsJsonAsync<ProductResponse>("/api/products/1", updateRequest);

// Example: Update product availability
await client.PatchAsJsonAsync("/api/products/1/availability", new { isAvailable = false });

// Example: Update product featured status
await client.PatchAsJsonAsync("/api/products/1/featured", new { isFeatured = true });

// Example: Delete a product
await client.DeleteAsync("/api/products/1");
```

## UsersController

Manages user authentication, registration, profile management, and user lifecycle operations including activation and deactivation. The controller provides RESTful endpoints for user management with proper authorization checks and response handling.

### Endpoints

- `POST /api/users/register` - Register a new user
- `POST /api/users/login` - Authenticate a user and return authentication token
- `GET /api/users/{id}` - Retrieve a user by ID
- `GET /api/users/profile` - Get the current authenticated user's profile
- `PUT /api/users/{id}` - Update a user's information
- `PUT /api/users/{id}/deactivate` - Deactivate a user account
- `PUT /api/users/{id}/activate` - Activate a user account
- `GET /api/users` - List all users in the system
- `GET /api/users/recently-active` - Get users who have been active within a specified time period

### Usage Example

```csharp
// Configure UserService in Program.cs
builder.Services.AddScoped<UserService>();

// Example: Register a new user
var registrationRequest = new CreateUserRequest
{
 Username = "johndoe",
 Email = "john.doe@example.com",
 Password = "SecurePassword123!",
 FirstName = "John",
 LastName = "Doe",
 Age = 30
};
var registerResponse = await client.PostAsJsonAsync<UserResponse>("/api/users/register", registrationRequest);
Console.WriteLine($"User registered: {registerResponse?.Username}");

// Example: Authenticate a user
var loginRequest = new LoginRequest
{
 Email = "john.doe@example.com",
 Password = "SecurePassword123!"
};
var loginResponse = await client.PostAsJsonAsync<LoginResponse>("/api/users/login", loginRequest);
Console.WriteLine($"Login successful. Token: {loginResponse?.Token?.Substring(0, 10)}...");

// Example: Get user profile (authenticated)
var profileResponse = await client.GetFromJsonAsync<UserResponse>("/api/users/profile");
Console.WriteLine($"User profile: {profileResponse?.Username}, Email: {profileResponse?.Email}");

// Example: Get a specific user by ID
var userResponse = await client.GetFromJsonAsync<UserResponse>("/api/users/1");
Console.WriteLine($"User: {userResponse?.Username}, Status: {userResponse?.Status}");

// Example: Update user information
var updateRequest = new UpdateUserRequest
{
 FirstName = "John",
 LastName = "Doe Updated",
 Email = "john.doe.updated@example.com",
 Age = 31
};
var updatedUser = await client.PutAsJsonAsync<UserResponse>("/api/users/1", updateRequest);
Console.WriteLine($"User updated: {updatedUser?.Username}");

// Example: Deactivate a user account
await client.PutAsync("/api/users/2/deactivate", null);
Console.WriteLine("User deactivated successfully");

// Example: Activate a user account
await client.PutAsync("/api/users/2/activate", null);
Console.WriteLine("User activated successfully");

// Example: List all users
var allUsers = await client.GetFromJsonAsync<List<UserResponse>>("/api/users");
Console.WriteLine($"Total users: {allUsers?.Count}");

// Example: Get recently active users (last 30 days)
var activeUsers = await client.GetFromJsonAsync<List<UserResponse>>("/api/users/recently-active");
Console.WriteLine($"Recently active users: {activeUsers?.Count}");

// Example: Get recently active users with custom time period
var recentActiveUsers = await client.GetFromJsonAsync<List<UserResponse>>("/api/users/recently-active?days=7");
Console.WriteLine($"Active in last 7 days: {recentActiveUsers?.Count}");
```

## WebhooksController

Receives and processes webhooks from external services including payment providers, email services, shipping providers, and custom integrations. Validates HMAC signatures, queues webhook payloads for asynchronous processing, and returns immediate HTTP responses. Designed to respond quickly (under 5 seconds) to avoid webhook timeouts from external providers.

### Endpoints

- `POST /api/webhooks/payment` - Handle payment provider webhooks
- `POST /api/webhooks/email` - Handle email service webhooks (delivery status, bounces, complaints)
- `POST /api/webhooks/shipping` - Handle shipping provider webhooks (tracking updates)
- `POST /api/webhooks/{provider}` - Generic endpoint for custom integrations

### Usage Example

```csharp
// Configure webhook endpoints in your application
// Add to Program.cs:
builder.Services.AddScoped<WebhookHandler>();

// Example: Sending a webhook from an external service
// Payment provider sends webhook to your endpoint:
POST https://yourapp.com/api/webhooks/payment
Content-Type: application/json
X-Signature: sha256=<HMAC-Signature>

{
  "payload": "{\"transactionId\":\"12345\",\"amount\":100.50,\"status\":\"completed\"}",
  "signature": "sha256=<HMAC-Signature>"
}

// Email service sends webhook to your endpoint:
POST https://yourapp.com/api/webhooks/email
Content-Type: application/json
X-Signature: sha256=<HMAC-Signature>

{
  "payload": "{\"event\":\"bounce\",\"email\":\"user@example.com\",\"recipient\":\"sender@example.com\"}",
  "signature": "sha256=<HMAC-Signature>"
}

// Shipping provider sends webhook to your endpoint:
POST https://yourapp.com/api/webhooks/shipping
Content-Type: application/json
X-Signature: sha256=<HMAC-Signature>

{
  "payload": "{\"trackingNumber\":\"UPS123456789\",\"status\":\"delivered\"}",
  "signature": "sha256=<HMAC-Signature>"
}

// Custom integration webhook:
POST https://yourapp.com/api/webhooks/custom-provider
Content-Type: application/json
X-Signature: sha256=<HMAC-Signature>

{
  "payload": "{\"event\":\"data-sync\",\"data\":{...}}",
  "signature": "sha256=<HMAC-Signature>"
}

// Response from controller:
HTTP/1.1 200 OK
Content-Type: application/json

{
  "acknowledged": true,
  "message": "Webhook received and queued for processing"
}
```

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

## ValidationHelper

Provides a comprehensive set of validation methods for common validation scenarios including null checks, range validation, string length validation, pattern matching, email validation, phone number validation, collection validation, and equality checks. This utility centralizes validation logic to ensure consistent error handling across controllers, services, and domain models.

### Usage Example

```csharp

// In a controller action
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
try
{
// Validate input parameters
ValidationHelper.NotNull(request, nameof(request));
ValidationHelper.NotNullOrEmpty(request.Username, nameof(request.Username));
ValidationHelper.LengthBetween(request.Username, 3, 50, nameof(request.Username));
ValidationHelper.ValidEmail(request.Email, nameof(request.Email));
ValidationHelper.InRange(request.Age, 18, 120, nameof(request.Age));
ValidationHelper.Equal(request.Password, request.ConfirmPassword, nameof(request.ConfirmPassword));
ValidationHelper.MaxItems(request.Roles, 5, nameof(request.Roles));

// Process valid request
var user = _userService.CreateUser(request);
return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
catch (ValidationException ex)
{
// Return validation errors to client
return BadRequest(new { errors = ex.Errors });
}
}
```

## DataExportHelper

Provides utilities for exporting data collections to various formats (CSV, JSON, XML) with support for format negotiation, custom field selection, and file naming. The helper centralizes data export logic to ensure consistent formatting and file handling across controllers and services.

### Usage Example

```csharp
// Define a sample model
public class UserDto
{
public int Id { get; set; }
public string? Username { get; set; }
public string? Email { get; set; }
public DateTime CreatedAt { get; set; }
}

// Sample data
var users = new List<UserDto>
{
new UserDto { Id = 1, Username = "johndoe", Email = "john@example.com", CreatedAt = DateTime.UtcNow.AddDays(-1) },
new UserDto { Id = 2, Username = "janedoe", Email = "jane@example.com", CreatedAt = DateTime.UtcNow.AddDays(-2) },
new UserDto { Id = 3, Username = "bobsmith", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-3) }
};

// Export to JSON format (default)
var jsonExport = DataExportHelper.ExportData(users, ExportFormat.Json);
Console.WriteLine($"JSON Export: {jsonExport.ContentType}, {jsonExport.FileName}");
File.WriteAllBytes(jsonExport.FileName, jsonExport.Data);

// Export to CSV format
var csvExport = DataExportHelper.ExportData(users, ExportFormat.Csv);
Console.WriteLine($"CSV Export: {csvExport.ContentType}, {csvExport.FileName}");
File.WriteAllBytes(csvExport.FileName, csvExport.Data);

// Export to XML format
var xmlExport = DataExportHelper.ExportData(users, ExportFormat.Xml);
Console.WriteLine($"XML Export: {xmlExport.ContentType}, {xmlExport.FileName}");
File.WriteAllBytes(xmlExport.FileName, xmlExport.Data);

// Negotiate format from Accept header
var acceptHeader = "text/csv";
var negotiatedFormat = DataExportHelper.NegotiateFormat(acceptHeader);
Console.WriteLine($"Negotiated format: {negotiatedFormat}");

// Get file extension and content type
var extension = DataExportHelper.GetFileExtension(ExportFormat.Json);
var contentType = DataExportHelper.GetContentType(ExportFormat.Csv);
Console.WriteLine($"JSON extension: {extension}, CSV content type: {contentType}");

// Export directly to FileContentResult for HTTP response
var httpContext = new DefaultHttpContext();
httpContext.Request.Headers["Accept"] = "application/json";
var fileResult = users.ExportAsFile(httpContext, "users-list");
Console.WriteLine($"File download name: {fileResult.FileDownloadName}");
```

## ValidationHelper

Provides a comprehensive set of validation methods for common validation scenarios including null checks, range validation, string length validation, pattern matching, email validation, phone number validation, collection validation, and equality checks. This utility centralizes validation logic to ensure consistent error handling across controllers, services, and domain models.

### Usage Example

```csharp
// In a controller action
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
  try
  {
    // Validate input parameters
    ValidationHelper.NotNull(request, nameof(request));
    ValidationHelper.NotNullOrEmpty(request.Username, nameof(request.Username));
    ValidationHelper.LengthBetween(request.Username, 3, 50, nameof(request.Username));
    ValidationHelper.ValidEmail(request.Email, nameof(request.Email));
    ValidationHelper.InRange(request.Age, 18, 120, nameof(request.Age));
    ValidationHelper.Equal(request.Password, request.ConfirmPassword, nameof(request.ConfirmPassword));
    ValidationHelper.MaxItems(request.Roles, 5, nameof(request.Roles));

    // Process valid request
    var user = _userService.CreateUser(request);
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
  }
  catch (ValidationException ex)
  {
    // Return validation errors to client
    return BadRequest(new { errors = ex.Errors });
  }
}

// In a service method
public void ProcessOrder(Order order)
{
  ValidationHelper.NotNull(order, nameof(order));
  ValidationHelper.NotNull(order.Customer, nameof(order.Customer));
  ValidationHelper.NotEmpty(order.Items, nameof(order.Items));
  ValidationHelper.InRange(order.TotalAmount, 0.01m, 10000m, nameof(order.TotalAmount));
  ValidationHelper.MatchesPattern(order.PhoneNumber, @"^\+?[0-9\s\-\(\)]{10,15}$", nameof(order.PhoneNumber), "Invalid phone number format");

  // Process valid order
  _orderRepository.Add(order);
}

// Validate email and phone number
ValidationHelper.ValidEmail("user@example.com");
ValidationHelper.ValidPhoneNumber("+1 (555) 123-4567");

// Validate collection length
var tags = new List<string> { "csharp", "aspnet", "react" };
ValidationHelper.MaxItems(tags, 10, nameof(tags));

// Batch validation with error collection
var validationErrors = ValidationHelper.ValidateAndCollectErrors(() => new Dictionary<string, string>
{
  [nameof(request.Username)] = request.Username,
  [nameof(request.Email)] = request.Email,
  [nameof(request.Age)] = request.Age.ToString(),
  [nameof(request.Password)] = request.Password,
  [nameof(request.ConfirmPassword)] = request.ConfirmPassword
});

if (validationErrors.Count > 0)
{
  throw new ValidationException(validationErrors);
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

## PerformanceHelper

Provides comprehensive performance measurement and profiling utilities for benchmarking operations, measuring execution time, memory usage, and query performance. The helper includes synchronous and asynchronous timing utilities, memory profiling, and detailed benchmarking statistics to help identify performance bottlenecks and optimize application performance.

### Usage Example

```csharp
// Measure synchronous operation execution time
var (result, elapsedMs) = PerformanceHelper.MeasureTime(() => 
{
    // Simulate CPU-intensive work
    Thread.Sleep(100);
    return "Operation completed";
});

Console.WriteLine($"Result: {result}, Time: {elapsedMs}ms");

// Measure asynchronous operation execution time
var (asyncResult, asyncElapsedMs) = await PerformanceHelper.MeasureTimeAsync(async () => 
{
    // Simulate async work
    await Task.Delay(150);
    return 42;
});

Console.WriteLine($"Async result: {asyncResult}, Time: {asyncElapsedMs}ms");

// Measure memory allocation
var (memoryResult, memoryBytes) = PerformanceHelper.MeasureMemory(() => 
{
    var list = new List<int>();
    for (int i = 0; i < 1000; i++)
    {
        list.Add(i);
    }
    return list;
});

Console.WriteLine($"Memory allocated: {memoryBytes} bytes");

// Get current memory usage
double currentMemoryMb = PerformanceHelper.GetMemoryUsageMb();
Console.WriteLine($"Current memory usage: {currentMemoryMb:F2} MB");

// Benchmark an operation with detailed statistics
var stats = PerformanceHelper.BenchmarkOperation(() => 
{
    // Simulate database query
    var data = new List<string>();
    for (int i = 0; i < 1000; i++)
    {
        data.Add($"Item {i}");
    }
    return data;
}, iterations: 10);

Console.WriteLine(stats);
Console.WriteLine($"Average time: {stats.AverageMs:F2}ms");
Console.WriteLine($"Min/Max: {stats.MinMs}/{stats.MaxMs}ms");
Console.WriteLine($"Throughput: {stats.ThroughputPerSecond:F2} ops/sec");

// Measure query performance
var (queryResults, queryTimeMs, count) = PerformanceHelper.MeasureQueryTime(() => 
{
    var query = Enumerable.Range(1, 5000)
        .Where(x => x % 2 == 0)
        .Select(x => x * 2)
        .ToList();
    return query;
});

Console.WriteLine($"Processed {count} items in {queryTimeMs}ms");

// Use IDisposable profiler for scoped performance measurement
using (var profiler = PerformanceHelper.Profile("Database query"))
{
    // Simulate database operation
    await Task.Delay(200);
}
// Console output: "Database query: 200ms"
```

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---


**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**


[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
