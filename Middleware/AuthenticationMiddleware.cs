// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using AspNetSpaTemplate.DTOs;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Middleware for API key and bearer token authentication.
/// Supports both header-based and query parameter API keys.
/// Sets up User principal for subsequent handlers.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // Store valid API keys in configuration (in production, use secure store like Azure Key Vault)
    private static readonly HashSet<string> ValidApiKeys = new()
    {
        "sk-dev-key-12345",
        "sk-test-key-67890"
    };

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Skip authentication for public endpoints
            if (IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Extract authentication token
            var token = ExtractAuthToken(context);

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Missing authentication token",
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            // Validate token (simplified - in production use JWT with proper signing)
            if (!ValidateToken(token))
            {
                _logger.LogWarning("Invalid authentication token attempted");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    ErrorCode = "INVALID_TOKEN",
                    Message = "Invalid or expired authentication token",
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            // Set user in context for downstream handlers
            context.Items["UserId"] = ExtractUserId(token);
            context.Items["AuthToken"] = token;

            _logger.LogInformation("User authenticated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error occurred");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                ErrorCode = "AUTH_ERROR",
                Message = "An error occurred during authentication"
            });
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts authentication token from header or query string.
    /// Checks for: Authorization: Bearer {token} or ?api_key={token}
    /// </summary>
    private static string? ExtractAuthToken(HttpContext context)
    {
        // Check Authorization header first
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var parts = authHeader.ToString().Split(' ');
            if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return parts[1];
        }

        // Check for API key in query string (less secure, only for testing)
        if (context.Request.Query.TryGetValue("api_key", out var apiKey))
            return apiKey.ToString();

        return null;
    }

    /// <summary>
    /// Validates that the token is in our allowed list.
    /// In production, verify JWT signature and expiration.
    /// </summary>
    private static bool ValidateToken(string token)
    {
        // For development: simple API key validation
        if (ValidApiKeys.Contains(token))
            return true;

        // In production, implement JWT validation here:
        // - Verify signature using public key
        // - Check expiration (exp claim)
        // - Validate issuer and audience
        return false;
    }

    /// <summary>
    /// Extracts user ID from token (for demonstration).
    /// In production, extract from JWT claims.
    /// </summary>
    private static int ExtractUserId(string token)
    {
        // Simplified: in production extract from JWT claims
        return token.GetHashCode() % 100; // Dummy implementation
    }

    /// <summary>
    /// Determines if endpoint should skip authentication.
    /// Health checks, login endpoints, etc. don't require auth.
    /// </summary>
    private static bool IsPublicEndpoint(PathString path)
    {
        var publicPaths = new[]
        {
            "/health",
            "/swagger",
            "/api/users/login",
            "/api/users/register",
            "/index.html"
        };

        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
        return publicPaths.Any(p => pathValue.Contains(p));
    }
}
