#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Extension methods for <see cref="NotFoundException"/> that provide convenient ways to create and work with not-found exceptions.
/// </summary>
public static class NotFoundExceptionExtensions
{
    /// <summary>
    /// Creates a <see cref="NotFoundException"/> for a resource that was not found.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <returns>A new <see cref="NotFoundException"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/></exception>
    public static NotFoundException ToNotFound(this string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return new NotFoundException(message);
    }

    /// <summary>
    /// Creates a <see cref="NotFoundException"/> for a specific resource type and ID.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The ID of the resource that was not found.</param>
    /// <returns>A new <see cref="NotFoundException"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resourceType"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="resourceType"/> is empty or whitespace.</exception>
    public static NotFoundException ToNotFound(this string resourceType, object resourceId)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        return new NotFoundException(resourceType, resourceId);
    }

    /// <summary>
    /// Creates a <see cref="NotFoundException"/> with a formatted message using the resource type and ID.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The ID of the resource that was not found.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>A new <see cref="NotFoundException"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resourceType"/> or <paramref name="format"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="resourceType"/> is empty or whitespace.</exception>
    /// <exception cref="FormatException">The format operation throws an error.</exception>
    public static NotFoundException ToNotFound(
        this string resourceType,
        object resourceId,
        string format,
        params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        ArgumentNullException.ThrowIfNull(format);

        var message = string.Format(CultureInfo.InvariantCulture, format, args);
        return new NotFoundException(message);
    }

    /// <summary>
    /// Creates a <see cref="NotFoundException"/> with an inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <returns>A new <see cref="NotFoundException"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> or <paramref name="innerException"/> is <see langword="null"/></exception>
    public static NotFoundException ToNotFound(
        this string message,
        Exception innerException)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);
        return new NotFoundException(message, innerException);
    }

    /// <summary>
    /// Creates a <see cref="NotFoundException"/> for a resource that was not found, using the resource's type name.
    /// </summary>
    /// <typeparam name="T">The type of resource that was not found.</typeparam>
    /// <param name="resourceId">The ID of the resource that was not found.</param>
    /// <returns>A new <see cref="NotFoundException"/> instance.</returns>
    public static NotFoundException ToNotFound<T>(this object resourceId)
    {
        var resourceType = typeof(T).Name;
        return new NotFoundException(resourceType, resourceId);
    }

    /// <summary>
    /// Determines whether this exception represents a not-found error for the specified resource type.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <param name="resourceType">The resource type to check for.</param>
    /// <returns><see langword="true"/> if this exception is for the specified resource type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="resourceType"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="resourceType"/> is empty or whitespace.</exception>
    public static bool IsNotFoundFor(
        this NotFoundException exception,
        string resourceType)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        return string.Equals(
            exception.ResourceType,
            resourceType,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether this exception represents a not-found error for the specified resource type and ID.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <param name="resourceType">The resource type to check for.</param>
    /// <param name="resourceId">The resource ID to check for.</param>
    /// <returns><see langword="true"/> if this exception matches both the resource type and ID; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/>, <paramref name="resourceType"/>, or <paramref name="resourceId"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="resourceType"/> is empty or whitespace.</exception>
    public static bool IsNotFoundFor(
        this NotFoundException exception,
        string resourceType,
        object resourceId)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        ArgumentNullException.ThrowIfNull(resourceId);

        return string.Equals(
                exception.ResourceType,
                resourceType,
                StringComparison.Ordinal)
            && Equals(exception.ResourceId, resourceId);
    }

    /// <summary>
    /// Determines whether this exception represents a not-found error for the specified resource type.
    /// </summary>
    /// <typeparam name="T">The type of resource to check for.</typeparam>
    /// <param name="exception">The exception to check.</param>
    /// <returns><see langword="true"/> if this exception is for the specified resource type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    public static bool IsNotFoundFor<T>(this NotFoundException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var expectedType = typeof(T).Name;
        return string.Equals(
            exception.ResourceType,
            expectedType,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the resource type from this exception, or throws if it's not set.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The resource type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The exception doesn't have a resource type set.</exception>
    public static string GetResourceType(this NotFoundException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.ResourceType
            ?? throw new InvalidOperationException(
                "The exception does not have a resource type set.");
    }

    /// <summary>
    /// Gets the resource ID from this exception, or throws if it's not set.
    /// </summary>
    /// <typeparam name="T">The expected type of the resource ID.</typeparam>
    /// <param name="exception">The exception.</param>
    /// <returns>The resource ID.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The exception doesn't have a resource ID set.</exception>
    public static T GetResourceId<T>(this NotFoundException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception.ResourceId is T typedId)
        {
            return typedId;
        }

        throw new InvalidOperationException(
            "The exception's resource ID is not of the expected type.");
    }
}