#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides validation helpers for parameters used with <see cref="NotFoundExceptionExtensions"/> extension methods.
/// </summary>
public static class NotFoundExceptionExtensionsValidation
{
    /// <summary>
    /// Validates the message parameter for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string)"/> method.
    /// </summary>
    /// <param name="message">The exception message to validate.</param>
    /// <returns>A list of validation problems; empty if the message is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    public static IReadOnlyList<string> ValidateMessage(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the message parameter is valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string)"/> method.
    /// </summary>
    /// <param name="message">The exception message to check.</param>
    /// <returns>True if the message is valid; otherwise, false.</returns>
    public static bool IsMessageValid(string message)
    {
        try
        {
            ValidateMessage(message);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the message parameter is valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string)"/> method,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="message">The exception message to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the message is not valid.</exception>
    public static void EnsureMessageValid(string message)
    {
        try
        {
            ValidateMessage(message);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                "Message parameter is not valid for NotFoundExceptionExtensions.ToNotFound method.",
                nameof(message),
                ex);
        }
    }

    /// <summary>
    /// Validates the resourceType and resourceId parameters for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, object)"/> method.
    /// </summary>
    /// <param name="resourceType">The resource type to validate.</param>
    /// <param name="resourceId">The resource ID to validate.</param>
    /// <returns>A list of validation problems; empty if both parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resourceType"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="resourceType"/> is empty or whitespace.</exception>
    public static IReadOnlyList<string> ValidateResource(string resourceType, object resourceId)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the resourceType and resourceId parameters are valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, object)"/> method.
    /// </summary>
    /// <param name="resourceType">The resource type to check.</param>
    /// <param name="resourceId">The resource ID to check.</param>
    /// <returns>True if both parameters are valid; otherwise, false.</returns>
    public static bool IsResourceValid(string resourceType, object resourceId)
    {
        try
        {
            ValidateResource(resourceType, resourceId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the resourceType and resourceId parameters are valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, object)"/> method,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="resourceType">The resource type to validate.</param>
    /// <param name="resourceId">The resource ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are not valid.</exception>
    public static void EnsureResourceValid(string resourceType, object resourceId)
    {
        try
        {
            ValidateResource(resourceType, resourceId);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                "Resource type and resource ID parameters are not valid for NotFoundExceptionExtensions.ToNotFound method.",
                nameof(resourceType),
                ex);
        }
    }

    /// <summary>
    /// Validates the resourceType, resourceId, format, and args parameters for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, object, string, object?[])"/> method.
    /// </summary>
    /// <param name="resourceType">The resource type to validate.</param>
    /// <param name="resourceId">The resource ID to validate.</param>
    /// <param name="format">The format string to validate.</param>
    /// <param name="args">The format arguments to validate.</param>
    /// <returns>A list of validation problems; empty if all parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resourceType"/>, <paramref name="format"/>, or <paramref name="args"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="resourceType"/> is empty or whitespace.</exception>
    public static IReadOnlyList<string> ValidateFormattedResource(string resourceType, object resourceId, string format, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        ArgumentNullException.ThrowIfNull(format);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the resourceType, resourceId, format, and args parameters are valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, object, string, object?[])"/> method.
    /// </summary>
    /// <param name="resourceType">The resource type to check.</param>
    /// <param name="resourceId">The resource ID to check.</param>
    /// <param name="format">The format string to check.</param>
    /// <param name="args">The format arguments to check.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsFormattedResourceValid(string resourceType, object resourceId, string format, params object?[] args)
    {
        try
        {
            ValidateFormattedResource(resourceType, resourceId, format, args);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the resourceType, resourceId, format, and args parameters are valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, object, string, object?[])"/> method,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="resourceType">The resource type to validate.</param>
    /// <param name="resourceId">The resource ID to validate.</param>
    /// <param name="format">The format string to validate.</param>
    /// <param name="args">The format arguments to validate.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are not valid.</exception>
    public static void EnsureFormattedResourceValid(string resourceType, object resourceId, string format, params object?[] args)
    {
        try
        {
            ValidateFormattedResource(resourceType, resourceId, format, args);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                "Resource type, resource ID, format, and args parameters are not valid for NotFoundExceptionExtensions.ToNotFound method.",
                nameof(resourceType),
                ex);
        }
    }

    /// <summary>
    /// Validates the message and innerException parameters for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, Exception)"/> method.
    /// </summary>
    /// <param name="message">The exception message to validate.</param>
    /// <param name="innerException">The inner exception to validate.</param>
    /// <returns>A list of validation problems; empty if both parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> or <paramref name="innerException"/> is null.</exception>
    public static IReadOnlyList<string> ValidateWithInner(string message, Exception innerException)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(innerException);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the message and innerException parameters are valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, Exception)"/> method.
    /// </summary>
    /// <param name="message">The exception message to check.</param>
    /// <param name="innerException">The inner exception to check.</param>
    /// <returns>True if both parameters are valid; otherwise, false.</returns>
    public static bool IsWithInnerValid(string message, Exception innerException)
    {
        try
        {
            ValidateWithInner(message, innerException);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the message and innerException parameters are valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound(string, Exception)"/> method,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="message">The exception message to validate.</param>
    /// <param name="innerException">The inner exception to validate.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are not valid.</exception>
    public static void EnsureWithInnerValid(string message, Exception innerException)
    {
        try
        {
            ValidateWithInner(message, innerException);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                "Message and inner exception parameters are not valid for NotFoundExceptionExtensions.ToNotFound method.",
                nameof(message),
                ex);
        }
    }

    /// <summary>
    /// Validates the resourceId parameter for use with <see cref="NotFoundExceptionExtensions.ToNotFound{T}(object)"/> method.
    /// </summary>
    /// <param name="resourceId">The resource ID to validate.</param>
    /// <returns>A list of validation problems; empty if the resource ID is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resourceId"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGeneric(object resourceId)
    {
        ArgumentNullException.ThrowIfNull(resourceId);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the resourceId parameter is valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound{T}(object)"/> method.
    /// </summary>
    /// <param name="resourceId">The resource ID to check.</param>
    /// <returns>True if the resource ID is valid; otherwise, false.</returns>
    public static bool IsGenericValid(object resourceId)
    {
        try
        {
            ValidateGeneric(resourceId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the resourceId parameter is valid for use with <see cref="NotFoundExceptionExtensions.ToNotFound{T}(object)"/> method,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="resourceId">The resource ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the resource ID is not valid.</exception>
    public static void EnsureGenericValid(object resourceId)
    {
        try
        {
            ValidateGeneric(resourceId);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                "Resource ID parameter is not valid for NotFoundExceptionExtensions.ToNotFound{T} method.",
                nameof(resourceId),
                ex);
        }
    }
}
