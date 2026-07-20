using System.ComponentModel.DataAnnotations;

namespace AspNetSpaTemplate.Configuration;

/// <summary>
/// Configuration options for the AspNetSpaTemplate.
/// </summary>
public class AspnetSpaTemplateOptions
{
    public const string SectionName = "AspnetSpaTemplate";

    [Required]
    public string JwtSecret { get; set; } = string.Empty;

    [Range(60, 86400)]
    public int JwtExpiration { get; set; } = 3600;

    [Required]
    public string Environment { get; set; } = "Production";

    public RequestLoggingOptions RequestLogging { get; set; } = new();
    public WebhookOptions Webhooks { get; set; } = new();
}

public class RequestLoggingOptions
{
    public bool Enabled { get; set; } = true;

    [Required]
    public string VerbosityLevel { get; set; } = "Standard";

    public bool LogRequestHeaders { get; set; } = false;
    public bool LogResponseHeaders { get; set; } = false;
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;

    [Range(0, 10000)]
    public int SlowRequestThresholdMs { get; set; } = 1000;

    public List<string> ExcludedPaths { get; set; } = new();
}

public class WebhookOptions
{
    [Required]
    public string PaymentProviderSecret { get; set; } = string.Empty;

    [Required]
    public string EmailServiceSecret { get; set; } = string.Empty;

    [Required]
    public string ShippingProviderSecret { get; set; } = string.Empty;
}
