#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Extension methods for collections and enumerable operations.
/// Provides utility methods for batching, filtering, and pagination.
/// </summary>
/// <remarks>
/// This class is static to support extension methods and sealed to prevent instantiation.
/// </remarks>
public static class CollectionExtensions
{
    /// <summary>
    /// Chunks a collection into smaller batches of specified size.
    /// Useful for batch processing database operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to batch.</param>
    /// <param name="batchSize">The maximum size of each batch. Must be positive.</param>
    /// <returns>An enumerable of batches, each containing up to <paramref name="batchSize"/> items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is less than or equal to 0.</exception>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive");

        var batch = new List<T>();
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>();
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Checks if collection is null or empty.
    /// Cleaner than checking both conditions separately.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <returns><see langword="true"/> if the collection is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source is null || !source.Any();
    }

    /// <summary>
    /// Returns the enumerable or an empty collection if null.
    /// Prevents null reference exceptions in LINQ chains.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <returns>The original collection if not null; otherwise, an empty enumerable.</returns>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Returns distinct elements from a sequence by using a specified key selector function.
    /// More flexible than LINQ's DistinctBy for complex equality scenarios.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of key used for distinct comparison.</typeparam>
    /// <param name="source">The collection to process.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An enumerable that contains the distinct elements from the source.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var seenKeys = new HashSet<TKey>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Converts collection to paginated result.
    /// Centralizes pagination logic to prevent off-by-one errors.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to paginate.</param>
    /// <param name="pageNumber">The 1-based page number. Values less than 1 are treated as 1.</param>
    /// <param name="pageSize">The number of items per page. Values less than 1 are treated as 10.</param>
    /// <returns>A tuple containing the paginated items and the total count of all items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static (IEnumerable<T> Items, int Total) Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Try to get count without enumerating. If successful, we can optimize pagination.
        if (source.TryGetNonEnumeratedCount(out var count))
        {
            var skip = (pageNumber - 1) * pageSize;
            if (skip >= count)
            {
                return (Enumerable.Empty<T>(), count);
            }

            var items = source.Skip(skip).Take(pageSize);
            return (items, count);
        }

        // Fallback for one-shot enumerables: enumerate once to get both count and items
        // This is necessary because we can't use Skip/Take without enumerating from the start
        var list = source.ToList();
        var total = list.Count;
        var skipCount = (pageNumber - 1) * pageSize;
        if (skipCount >= total)
        {
            return (Enumerable.Empty<T>(), total);
        }

        var resultItems = list.Skip(skipCount).Take(pageSize);
        return (resultItems, total);
    }

    /// <summary>
    /// Applies action to each item in the collection (side effects).
    /// Used for logging, validation, or other non-mapping operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to process.</param>
    /// <param name="action">The action to apply to each item.</param>
    /// <returns>The original collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Converts dictionary to comma-separated key=value pairs.
    /// Useful for logging or debugging configuration values.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values.</typeparam>
    /// <param name="source">The dictionary to convert.</param>
    /// <returns>A string representation of the dictionary, or empty string if null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> source)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Count == 0
            ? string.Empty
            : string.Join(", ", source.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    /// <summary>
    /// Safely casts collection to another type, filtering out invalid items.
    /// More forgiving than direct cast for heterogeneous collections.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <param name="source">The collection to process.</param>
    /// <returns>An enumerable containing only items that can be cast to <typeparamref name="TTarget"/>.</returns>
    public static IEnumerable<TTarget> SafeCast<TSource, TTarget>(this IEnumerable<TSource> source)
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (var item in source)
        {
            if (item is TTarget target)
                yield return target;
        }
    }

    /// <summary>
    /// Shuffles collection using Fisher-Yates algorithm.
    /// Returns new collection, original is unchanged.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to shuffle.</param>
    /// <returns>A new collection with elements in random order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var items = source.ToList();

        for (int i = items.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Shared.Next(i + 1);
            (items[i], items[randomIndex]) = (items[randomIndex], items[i]);
        }

        return items;
    }
}
