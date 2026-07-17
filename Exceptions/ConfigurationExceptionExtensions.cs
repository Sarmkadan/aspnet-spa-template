#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AspNetSpaTemplate.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ConfigurationException"/> that enhance configuration exception handling
    /// with improved messaging, aggregation, and key-based operations.
    /// </summary>
    /// <remarks>
    /// This static class is implicitly sealed and cannot be inherited from.
    /// </remarks>
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
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
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

            var combinedMessage = string.Concat(
                exception.Message,
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
        /// If an exception has no configuration key or the key is empty/whitespace, it is not included in the dictionary.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exceptions"/> is null.</exception>
        public static IReadOnlyDictionary<string, string> GetConfigurationKeys(
            this IEnumerable<ConfigurationException> exceptions)
        {
            ArgumentNullException.ThrowIfNull(exceptions);

            return exceptions
                .Where(ex => ex.ConfigurationKey is { Length: > 0 })
                .ToDictionary(
                    ex => ex.ConfigurationKey!,
                    ex => ex.Message,
                    StringComparer.Ordinal);
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

            return exceptions.Any(ex =>
                string.Equals(ex.ConfigurationKey, configurationKey, StringComparison.Ordinal));
        }
    }
}
