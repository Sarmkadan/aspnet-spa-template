// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Extension methods for collections and enumerable operations.
/// Provides utility methods for batching, filtering, and pagination.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Chunks a collection into smaller batches of specified size.
    /// Useful for batch processing database operations.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

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
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Returns the enumerable or an empty collection if null.
    /// Prevents null reference exceptions in LINQ chains.
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Distils duplicate items based on selector and returns unique items.
    /// More flexible than DistinctBy for complex equality scenarios.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
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
    public static (IEnumerable<T> Items, int Total) Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var total = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return (items, total);
    }

    /// <summary>
    /// Applies action to each item in the collection (side effects).
    /// Used for logging, validation, or other non-mapping operations.
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
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
    public static string ToKeyValueString<TKey, TValue>(this Dictionary<TKey, TValue> source)
    {
        if (source == null || source.Count == 0)
            return string.Empty;

        var pairs = source.Select(kvp => $"{kvp.Key}={kvp.Value}");
        return string.Join(", ", pairs);
    }

    /// <summary>
    /// Safely casts collection to another type, filtering out invalid items.
    /// More forgiving than direct cast for heterogeneous collections.
    /// </summary>
    public static IEnumerable<TTarget> SafeCast<TSource, TTarget>(this IEnumerable<TSource> source) where TTarget : class
    {
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
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var items = source.ToList();
        var random = new Random();

        for (int i = items.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (items[i], items[randomIndex]) = (items[randomIndex], items[i]);
        }

        return items;
    }
}
