#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Configuration;

/// <summary>
/// Configuration root for the Progressive Web App subsystem.
/// Bind this class against the <c>Pwa</c> section of <c>appsettings.json</c>
/// via <see cref="OfflineSupportExtensions.AddOfflineSupport"/>.
/// </summary>
public sealed class PwaOptions
{
    /// <summary>The configuration section key used when binding from <c>appsettings.json</c>.</summary>
    public const string SectionName = "Pwa";

    /// <summary>
    /// VAPID key material used for Web Push authentication (RFC 8292).
    /// Generate a key pair once with a tool such as <c>web-push generate-vapid-keys</c>
    /// and store the private key in a secret store — never in source control.
    /// </summary>
    public VapidOptions Vapid { get; init; } = new();

    /// <summary>
    /// When <c>false</c>, all push notification delivery is silently skipped.
    /// Useful for test environments that lack outbound network access to push services.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnablePushNotifications { get; set; } = true;

    /// <summary>
    /// When <c>false</c>, the offline sync queue worker is still registered but performs
    /// no replay operations, leaving entries in <see cref="Models.SyncEntryStatus.Pending"/>.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableOfflineSync { get; set; } = true;

    /// <summary>
    /// Upper bound on the number of push subscription records processed in a single
    /// broadcast operation. Prevents runaway memory usage on large subscriber lists.
    /// Defaults to 200.
    /// </summary>
    public int MaxNotificationsPerBatch { get; set; } = 200;

    /// <summary>
    /// Maximum number of replay attempts for a single sync queue entry before it is
    /// permanently marked <see cref="Models.SyncEntryStatus.Failed"/>.
    /// Defaults to 5.
    /// </summary>
    public int MaxSyncRetries { get; set; } = 5;

    /// <summary>
    /// Base delay in seconds between sync replay retries.
    /// The actual delay is computed as <c>BaseDelay × 2^(attempt-1)</c> (exponential back-off).
    /// Defaults to 30 seconds.
    /// </summary>
    public int SyncRetryBaseDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Age threshold in hours beyond which completed or permanently-failed sync entries
    /// are purged from the database by the background worker.
    /// Defaults to 72 hours (3 days).
    /// </summary>
    public int SyncQueueMaxAgeHours { get; set; } = 72;

    /// <summary>
    /// Per-delivery HTTP timeout in seconds when posting to a browser push service endpoint.
    /// Defaults to 10 seconds.
    /// </summary>
    public int PushDeliveryTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Number of days after which subscriptions that have never received a successful delivery
    /// are considered stale and eligible for deactivation.
    /// Defaults to 30 days.
    /// </summary>
    public int InactiveSubscriptionPurgeDays { get; set; } = 30;

    /// <summary>
    /// Returns <c>true</c> when both a VAPID public and private key have been configured.
    /// Used at startup to warn operators that push delivery will not work without valid keys.
    /// </summary>
    public bool IsVapidConfigured =>
        !string.IsNullOrWhiteSpace(Vapid.PublicKey) &&
        !string.IsNullOrWhiteSpace(Vapid.PrivateKey);
}

/// <summary>
/// VAPID key material and operator contact address for Web Push authentication (RFC 8292).
/// The public key is also forwarded to the browser as the <c>applicationServerKey</c>
/// argument to <c>PushManager.subscribe()</c>.
/// </summary>
public sealed class VapidOptions
{
    /// <summary>
    /// The URL-safe base64-encoded uncompressed EC public key (P-256 curve, 65 bytes).
    /// Shared with clients via <c>GET /api/v1/pwa/status</c> so they can create
    /// subscriptions bound to this server.
    /// </summary>
    public string PublicKey { get; set; } = "";

    /// <summary>
    /// The URL-safe base64-encoded P-256 private key.
    /// Must be stored securely (environment variable, secrets manager) and never committed.
    /// </summary>
    public string PrivateKey { get; set; } = "";

    /// <summary>
    /// A <c>mailto:</c> or <c>https:</c> URI identifying the operator of this push service
    /// (RFC 8292 §2.1). Push services may use this to contact the operator about delivery issues.
    /// Defaults to a placeholder — override in production.
    /// </summary>
    public string Subject { get; set; } = "mailto:webmaster@example.com";
}
