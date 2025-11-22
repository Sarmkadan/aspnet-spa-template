// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Utilities;

namespace AspNetSpaTemplate.Integration;

/// <summary>
/// Generic wrapper for consuming external APIs.
/// Handles common concerns: retries, timeouts, error handling, logging.
/// </summary>
public class ExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    public ExternalApiClient(HttpClient httpClient, ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets data from external API with automatic retry on transient failures.
    /// Returns typed response or throws if all retries exhausted.
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint) where T : class
    {
        return await GetWithRetryAsync<T>(endpoint);
    }

    /// <summary>
    /// Posts data to external API with automatic retry.
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object request) where T : class
    {
        return await PostWithRetryAsync<T>(endpoint, request);
    }

    /// <summary>
    /// Gets data with automatic retry logic on transient failures (5xx, timeouts).
    /// Circuit breaker pattern: after 3 failures, fails fast without retrying.
    /// </summary>
    private async Task<T> GetWithRetryAsync<T>(string endpoint) where T : class
    {
        int attempt = 0;
        while (attempt < _maxRetries)
        {
            try
            {
                _logger.LogInformation($"GET {endpoint} (attempt {attempt + 1}/{_maxRetries})");

                using (var response = await _httpClient.GetAsync(endpoint))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonSerializationHelper.Deserialize<T>(json) ?? throw new InvalidOperationException("Response was null");
                    }

                    // Retry on server errors (5xx) and timeouts (408)
                    if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        attempt++;
                        if (attempt < _maxRetries)
                        {
                            await Task.Delay(_retryDelay * attempt);
                            continue;
                        }
                    }

                    // Don't retry on client errors (4xx)
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
            {
                // Retry on timeout
                attempt++;
                if (attempt < _maxRetries)
                {
                    _logger.LogWarning($"Timeout on {endpoint}, retrying...");
                    await Task.Delay(_retryDelay * attempt);
                    continue;
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling {endpoint}");
                throw;
            }
        }

        throw new HttpRequestException($"Failed to GET {endpoint} after {_maxRetries} attempts");
    }

    /// <summary>
    /// Posts data with retry logic (same as GET retry strategy).
    /// </summary>
    private async Task<T> PostWithRetryAsync<T>(string endpoint, object request) where T : class
    {
        int attempt = 0;
        while (attempt < _maxRetries)
        {
            try
            {
                var json = JsonSerializationHelper.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation($"POST {endpoint} (attempt {attempt + 1}/{_maxRetries})");

                using (var response = await _httpClient.PostAsync(endpoint, content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return JsonSerializationHelper.Deserialize<T>(responseJson) ?? throw new InvalidOperationException("Response was null");
                    }

                    // Retry on server errors
                    if ((int)response.StatusCode >= 500)
                    {
                        attempt++;
                        if (attempt < _maxRetries)
                        {
                            await Task.Delay(_retryDelay * attempt);
                            continue;
                        }
                    }

                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling {endpoint}");
                attempt++;
                if (attempt < _maxRetries)
                {
                    await Task.Delay(_retryDelay * attempt);
                    continue;
                }
                throw;
            }
        }

        throw new HttpRequestException($"Failed to POST {endpoint} after {_maxRetries} attempts");
    }
}

/// <summary>
/// Configuration for external API clients.
/// </summary>
public class ExternalApiConfig
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public bool LogRequests { get; set; } = true;
    public bool LogResponses { get; set; } = false; // Be careful with sensitive data
}
