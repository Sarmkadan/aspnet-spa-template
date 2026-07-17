# CollectionExtensions

CollectionExtensions provides a set of LINQ‑friendly extension methods that simplify common operations on sequences such as batching, null‑safe enumeration, distinct selection by key, pagination, side‑effect iteration, query‑string building, safe casting, and shuffling.

## API

### Batch<T>
```csharp
public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
```
Splits the source sequence into consecutive batches of the specified size.  
- **Parameters**  
  - `source`: The sequence to batch.  
  - `size`: The maximum number of elements per batch; must be greater than zero.  
- **Return value**  
  An `IEnumerable<IEnumerable<T>>` where each inner enumerable contains up to `size` elements from `source`. The final batch may contain fewer elements if the source length is not a multiple of `size`.  
- **Exceptions**  
  - `ArgumentNullException` if `source` is `null`.  
  - `ArgumentOutOfRangeException` if `size` is less than or equal to zero.

### IsNullOrEmpty<T>
```csharp
public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
```
Determines whether the source sequence is `null` or contains no elements.  
- **Parameters**  
  - `source`: The sequence to test.  
- **Return value**  
  `true` if `source` is `null` or empty; otherwise `false`.  
- **Exceptions**  
  None.

### OrEmpty<T>
```csharp
public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source)
```
Returns the source sequence if it is not `null`; otherwise returns an empty enumerable of the same element type.  
- **Parameters**  
  - `source`: The sequence that may be `null`.  
- **Return value**  
  `source` when non‑null; otherwise `Enumerable.Empty<T>()`.  
- **Exceptions**  
  None.

### DistinctBy<T, TKey>
```csharp
public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
```
Produces a sequence containing distinct elements based on a key selector, preserving the original order of first occurrence.  
- **Parameters**  
  - `source`: The sequence to filter.  
  - `keySelector`: A function that extracts the key used for comparison.  
- **Return value**  
  An `IEnumerable<T>` with duplicate keys removed.  
- **Exceptions**  
  - `ArgumentNullException` if `source` or `keySelector` is `null`.

### Paginate<T>
```csharp
public static (IEnumerable<T> Items, int Total) Paginate<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
```
Returns a page of items from the source together with the total count of items.  
- **Parameters**  
  - `source`: The sequence to paginate.  
  - `pageIndex`: Zero‑based index of the page to retrieve; must be non‑negative.  
  - `pageSize`: Number of items per page; must be greater than zero.  
- **Return value**  
  A value tuple where `Items` is an `IEnumerable<T>` containing the requested page and `Total` is the total number of elements in `source`.  
- **Exceptions**  
  - `ArgumentNullException` if `source` is `null`.  
  - `ArgumentOutOfRangeException` if `pageIndex` is negative or `pageSize` is less than or equal to zero.

### ForEach<T>
```csharp
public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
```
Executes the supplied action for each element in the sequence and then returns the original sequence to allow method chaining.  
- **Parameters**  
  - `source`: The sequence to iterate.  
  - `action`: The delegate to invoke for each element.  
- **Return value**  
  The original `source` sequence.  
- **Exceptions**  
  - `ArgumentNullException` if `source` or `action` is `null`.  
  - Any exception thrown by `action` is propagated to the caller.

### ToKeyValueString<TKey, TValue>
```csharp
public static string ToKeyValueString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, string separator = "&", string equals = "=")
```
Creates a URL‑encoded query string from a sequence of key‑value pairs.  
- **Parameters**  
  - `source`: The sequence of `KeyValuePair<TKey,TValue>` to encode.  
  - `separator`: String placed between pairs; defaults to `"&"`.  
  - `equals`: String placed between each key and its value; defaults to `"="`.  
- **Return value**  
  A single string where each pair is formatted as `key{equals}value` and pairs are joined by `separator`. Keys and values are converted via `ToString()` and then URL‑encoded.  
- **Exceptions**  
  - `ArgumentNullException` if `source` is `null`.  
  - `ArgumentNullException` if `separator` or `equals` is `null`.

### SafeCast<TSource, TTarget>
```csharp
public static IEnumerable<TTarget> SafeCast<TSource, TTarget>(this IEnumerable<TSource> source)
```
Attempts to cast each element of the source sequence to `TTarget`, returning only those elements that can be successfully cast; elements that cannot be cast are silently omitted.  
- **Parameters**  
  - `source`: The sequence to cast.  
- **Return value**  
  An `IEnumerable<TTarget>` containing the successfully cast elements in their original order.  
- **Exceptions**  
  - `ArgumentNullException` if `source` is `null`.  
  - Note: Invalid cast operations do not throw; they are filtered out.

### Shuffle<T>
```csharp
public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
```
Returns a new sequence with the elements of the source in a random order.  
- **Parameters**  
  - `source`: The sequence to shuffle.  
- **Return value**  
  An `IEnumerable<T>` containing all elements of `source` in a shuffled order. The operation uses a Fisher‑Yates shuffle algorithm.  
- **Exceptions**  
  - `ArgumentNullException` if `source` is `null`.  

## Usage

### Example 1: Batching and paging a list of products
```csharp
IEnumerable<Product> products = GetProducts(); // Assume this returns a list

// Show products in groups of 10 for a UI carousel
var batches = products.Batch(10);

// Get the second page (zero‑based) with page size 20
var page = products.Paginate(pageIndex: 1, pageSize: 20);
foreach (var p in page.Items)
{
    Console.WriteLine(p.Name);
}
Console.WriteLine($"Total products: {page.Total}");
```

### Example 2: Building a query string and safely casting mixed objects
```csharp
var parameters = new List<KeyValuePair<string, object>>
{
    new KeyValuePair<string, object>("page", 2),
    new KeyValuePair<string, object>("sort", "date"),
    new KeyValuePair<string, object>("filter", null)
};

// Only keep entries where the value is not null and can be cast to string
var stringParams = parameters
    .Where(kvp => kvp.Value != null)
    .SafeCast<object, string>()
    .Select(kvp => new KeyValuePair<string, string>(kvp.Key, (string)kvp.Value));

string query = stringParams.ToKeyValueString();
// Result might be: "page=2&sort=date"
```

## Notes
- All extension methods are **pure** with respect to the source sequence; they do not modify the original enumerable unless the method explicitly enumerates it (e.g., `Shuffle` creates a new ordered sequence).  
- Methods that accept a `Func<T, TResult>` or `Action<T>` will throw `ArgumentNullException` if the delegate is `null`.  
- `Batch`, `Paginate`, and `Shuffle` force enumeration of the source to compute their results; therefore, they are not suitable for infinite sequences.  
- `SafeCast` suppresses invalid cast exceptions; if you need to be notified of failed casts, consider using `Cast<TTarget>` and handling the exception yourself.  
- The extensions are stateless and thread‑safe for concurrent read‑only access to the source sequence, provided the source itself is not modified during enumeration. If the source is a mutable collection accessed by multiple threads, external synchronization is required.  
- `OrEmpty` is useful when chaining LINQ operators after a potentially null source, preventing `NullReferenceException` without explicit null checks.  
- `ToKeyValueString` performs URL encoding via `Uri.EscapeDataString` on the string representation of each key and value; ensure that the types used have meaningful `ToString` implementations.
