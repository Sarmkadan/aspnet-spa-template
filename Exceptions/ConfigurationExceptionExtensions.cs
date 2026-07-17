#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AspNetSpaTemplate.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="ConfigurationException"/> that provide additional functionality
    /// for working with configuration-related exceptions.
    /// </summary>
    public static class ConfigurationExceptionExtensions
    {
        /// <summary>
        /// Creates a new <see cref="ConfigurationException"/> with an improved error message that includes
        /// the configuration key and a formatted message.
        /// </summary>
        /// <param name="exception">The original exception.</param>
        /// <param name="format">The format string for the new message.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <returns>A new <see cref="ConfigurationException"/> with the formatted message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
        public static ConfigurationException WithMessage(
            this ConfigurationException exception,
            string format,
            params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(format);

            var message = string.Format(CultureInfo.InvariantCulture, format, args);

            return exception.ConfigurationKey is { } key
                ? new ConfigurationException(key, message)
                : new ConfigurationException(message);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationException"/> that combines this exception with another,
        /// preserving both configuration keys and creating a composite message.
        /// </summary>
        /// <param name="exception">The original exception.</param>
        /// <param name="additionalMessage">Additional context to append to the error message.</param>
        /// <returns>A new <see cref="ConfigurationException"/> with combined information.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="additionalMessage"/> is null.</exception>
        public static ConfigurationException CombineWith(
            this ConfigurationException exception,
            string additionalMessage)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(additionalMessage);

            var baseMessage = exception.Message;
            var combinedMessage = string.Concat(
                baseMessage,
                Environment.NewLine,
                "Additional context: ",
                additionalMessage);

            return exception.ConfigurationKey is { } key
                ? new ConfigurationException(key, combinedMessage)
                : new ConfigurationException(combinedMessage);
        }

        /// <summary>
        /// Gets a dictionary containing all configuration keys from a collection of exceptions.
        /// Useful for aggregating configuration issues from multiple sources.
        /// </summary>
        /// <param name="exceptions">The collection of configuration exceptions.</param>
        /// <returns>A read-only dictionary mapping configuration keys to exception messages.
        /// If an exception has no configuration key, it is not included in the dictionary.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exceptions"/> is null.</exception>
        public static IReadOnlyDictionary<string, string> GetConfigurationKeys(
            this IEnumerable<ConfigurationException> exceptions)
        {
            ArgumentNullException.ThrowIfNull(exceptions);

            var result = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var exception in exceptions)
            {
                if (exception.ConfigurationKey is { } key && !string.IsNullOrEmpty(key))
                {
                    result.TryAdd(key, exception.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether any exception in the collection has the specified configuration key.
        /// </summary>
        /// <param name="exceptions">The collection of configuration exceptions.</param>
        /// <param name="configurationKey">The configuration key to search for.</param>
        /// <returns>True if any exception has the specified key; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exceptions"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="configurationKey"/> is null.</exception>
        public static bool HasConfigurationKey(
            this IEnumerable<ConfigurationException> exceptions,
            string configurationKey)
        {
            ArgumentNullException.ThrowIfNull(exceptions);
            ArgumentNullException.ThrowIfNull(configurationKey);

            foreach (var exception in exceptions)
            {
                if (string.Equals(
                    exception.ConfigurationKey,
                    configurationKey,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}