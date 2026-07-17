using System;
using System.Collections.Generic;
using System.Globalization;

namespace AspNetSpaTemplate.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ConfigurationException"/> that offer useful
    /// functionality for exception handling and analysis in ASP.NET Core SPA template scenarios.
    /// </summary>
    public static class AspNetSpaTemplateExceptionExtensions
    {
        /// <summary>
        /// Gets the configuration key associated with the exception, if any.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>The configuration key, or null if not present.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
        public static string? GetConfigurationKey(this ConfigurationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.ConfigurationKey;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationException"/> with the specified message,
        /// preserving the original configuration key if present.
        /// </summary>
        /// <param name="exception">The original exception.</param>
        /// <param name="message">The new exception message.</param>
        /// <returns>A new exception instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> or <paramref name="message"/> is null.</exception>
        public static ConfigurationException WithMessage(
            this ConfigurationException exception,
            string message)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(message);

            return exception.ConfigurationKey is { } key
                ? new ConfigurationException(key, message)
                : new ConfigurationException(message);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationException"/> with the specified configuration key and message,
        /// preserving any existing configuration key as a composite key.
        /// </summary>
        /// <param name="exception">The original exception.</param>
        /// <param name="configurationKey">The configuration key to associate.</param>
        /// <param name="message">The new exception message.</param>
        /// <returns>A new exception instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/>, <paramref name="configurationKey"/>, or <paramref name="message"/> is null.</exception>
        public static ConfigurationException WithConfigurationKey(
            this ConfigurationException exception,
            string configurationKey,
            string message)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(configurationKey);
            ArgumentNullException.ThrowIfNull(message);

            return exception.ConfigurationKey is { } existingKey
                ? new ConfigurationException($"{existingKey}.{configurationKey}", message)
                : new ConfigurationException(configurationKey, message);
        }

        /// <summary>
        /// Gets a collection of all configuration keys involved in the exception chain,
        /// ordered from most specific to least specific.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>An enumerable of configuration keys, or empty if none.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
        public static IEnumerable<string> GetConfigurationKeyHierarchy(this ConfigurationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (exception.ConfigurationKey is not { } key)
            {
                yield break;
            }

            yield return key;
        }
    }
}