// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Integration;

/// <summary>
/// Interface for creating and managing HTTP clients for external API calls.
/// Encapsulates HTTP configuration, retry policies, and timeout handling.
/// Prevents socket exhaustion through proper HttpClient reuse.
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Gets or creates an HTTP client for the specified service.
    /// Clients are reused to prevent socket starvation.
    /// </summary>
    HttpClient GetClient(string serviceName);

    /// <summary>
    /// Gets or creates an HTTP client with custom configuration.
    /// </summary>
    HttpClient GetClient(string serviceName, Action<HttpClient> configure);
}

/// <summary>
/// Default implementation of HTTP client factory.
/// Manages client lifecycle and configuration per service.
/// </summary>
public class DefaultHttpClientFactory : IHttpClientFactory
{
    private readonly Dictionary<string, HttpClient> _clients = new();
    private readonly object _lock = new();
    private readonly ILogger<DefaultHttpClientFactory> _logger;

    public DefaultHttpClientFactory(ILogger<DefaultHttpClientFactory> logger)
    {
        _logger = logger;
    }

    public HttpClient GetClient(string serviceName)
    {
        return GetClient(serviceName, _ => { });
    }

    public HttpClient GetClient(string serviceName, Action<HttpClient> configure)
    {
        lock (_lock)
        {
            if (_clients.TryGetValue(serviceName, out var client))
                return client;

            // Create new client with defaults
            var newClient = new HttpClient(new HttpClientHandler
            {
                // Use HTTP/2 for better performance
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            })
            {
                // Reasonable default timeout (30 seconds)
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Add User-Agent to identify requests
            newClient.DefaultRequestHeaders.Add("User-Agent", "AspNetSpaTemplate/1.0");
            newClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Apply custom configuration
            configure(newClient);

            _clients[serviceName] = newClient;
            _logger.LogInformation($"Created HTTP client for service: {serviceName}");

            return newClient;
        }
    }

    /// <summary>
    /// Disposes all managed HTTP clients.
    /// Call this during application shutdown.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var client in _clients.Values)
            {
                client?.Dispose();
            }
            _clients.Clear();
        }
    }
}

/// <summary>
/// Extension methods for HTTP clients.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Sends GET request and deserializes JSON response.
    /// Throws HttpRequestException on non-success status code.
    /// </summary>
    public static async Task<T?> GetAsJsonAsync<T>(this HttpClient client, string url) where T : class
    {
        using (var response = await client.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return Utilities.JsonSerializationHelper.Deserialize<T>(json);
        }
    }

    /// <summary>
    /// Sends POST request with JSON body and deserializes response.
    /// </summary>
    public static async Task<T?> PostAsJsonAsync<T>(this HttpClient client, string url, object body) where T : class
    {
        var json = Utilities.JsonSerializationHelper.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using (var response = await client.PostAsync(url, content))
        {
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return Utilities.JsonSerializationHelper.Deserialize<T>(responseJson);
        }
    }

    /// <summary>
    /// Safely sends GET request (doesn't throw on non-success codes).
    /// Returns (IsSuccess, StatusCode, Content).
    /// </summary>
    public static async Task<(bool IsSuccess, int StatusCode, string Content)> SafeGetAsync(this HttpClient client, string url)
    {
        try
        {
            using (var response = await client.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, (int)response.StatusCode, content);
            }
        }
        catch (HttpRequestException ex)
        {
            return (false, 0, ex.Message);
        }
    }
}
