#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Manages per-user UI theme preferences so the server can pre-render the
/// correct theme class before the client-side JavaScript executes, eliminating
/// the flash of unstyled content on page load.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    /// <summary>
    /// Returns the saved colour scheme for the given user.
    /// Returns <see cref="ColourScheme.System"/> when no explicit preference has been stored.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task<ColourScheme> GetSchemeAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Persists the user's explicit theme choice.
    /// Passing <see cref="ColourScheme.System"/> clears any saved preference so the
    /// OS-level setting takes effect on the client.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="scheme">The chosen colour scheme.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task SetSchemeAsync(int userId, ColourScheme scheme, CancellationToken ct = default);

    /// <summary>
    /// Removes any saved preference for the given user, resetting to
    /// <see cref="ColourScheme.System"/>.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task ClearSchemeAsync(int userId, CancellationToken ct = default);
}

/// <summary>Represents the supported UI colour schemes.</summary>
public enum ColourScheme
{
    /// <summary>Follow the operating system / browser preference.</summary>
    System = 0,

    /// <summary>Always use the light theme.</summary>
    Light = 1,

    /// <summary>Always use the dark theme.</summary>
    Dark = 2
}

/// <summary>
/// Event arguments for the ThemeChanged event.
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// The ID of the user whose theme changed.
    /// </summary>
    public int UserId { get; }

    /// <summary>
    /// The new theme scheme.
    /// </summary>
    public ColourScheme Scheme { get; }

    /// <summary>
    /// Initializes a new instance of the ThemeChangedEventArgs class.
    /// </summary>
    /// <param name="userId">The ID of the user whose theme changed.</param>
    /// <param name="scheme">The new theme scheme.</param>
    public ThemeChangedEventArgs(int userId, ColourScheme scheme)
    {
        UserId = userId;
        Scheme = scheme;
    }
}
