# CsvFormatter

The `CsvFormatter` type provides utilities for serializing objects to CSV format and parsing CSV data back into a list of dictionaries. It is intended for simple export/import scenarios where a tabular representation of .NET objects is required, allowing customization of delimiter, header inclusion, and formatting of dates and currencies.

## API

### `public static string ToCsv<T>(IEnumerable<T> items)`
- **Purpose**: Serializes a collection of objects of type `T` to a CSV‑encoded string.
- **Parameters**:
  - `items`: The sequence of objects to serialize. Each public readable property of `T` becomes a column.
- **Return value**: A string containing the CSV data, using the current delimiter and header settings.
- **Exceptions**:
  - `ArgumentNullException` if `items` is `null`.
  - `InvalidOperationException` if a property of `T` cannot be read (e.g., indexer‑only property).

### `public static byte[] ToCsvBytes<T>(IEnumerable<T> items)`
- **Purpose**: Serializes a collection of objects of type `T` to a CSV‑encoded byte array (UTF‑8).
- **Parameters**:
  - `items`: The sequence of objects to serialize.
- **Return value**: A byte array containing the CSV data.
- **Exceptions**:
  - `ArgumentNullException` if `items` is `null`.
  - `InvalidOperationException` if a property of `T` cannot be read.

### `public static List<Dictionary<string, string>> ParseCsv(string csv)`
- **Purpose**: Parses a CSV string into a list where each entry represents a row as a dictionary mapping column names to cell values.
- **Parameters**:
  - `csv`: The CSV text to parse. The first line is treated as the header unless `IncludeHeader` is set to `false`.
- **Return value**: A `List<Dictionary<string, string>>` where each dictionary corresponds to a row.
- **Exceptions**:
  - `ArgumentNullException` if `csv` is `null`.
  - `FormatException` if the CSV is malformed (e.g., inconsistent number of columns) or if a quoted field is not properly closed.

### `public bool IncludeHeader`
- **Purpose**: Gets or sets whether the first line of the CSV output should contain column names.
- **Default value**: `true`.
- **Remarks**: When `false`, `ParseCsv` will generate column names as `"Column0"`, `"Column1"`, … based on ordinal position.

### `public string Delimiter`
- **Purpose**: Gets or sets the string used to separate fields in the CSV output.
- **Default value**: `","` (comma).
- **Remarks**: Common alternatives include `"\t"` for tab‑separated values or `";"` for locales that use the comma as a decimal separator.

### `public string? DateFormat`
- **Purpose**: Gets or sets the format string applied to `DateTime` and `DateTimeOffset` properties when serializing to CSV.
- **Default value**: `null`, which uses the default `ToString()` representation.
- **Remarks**: If set, the formatter invokes `value.ToString(DateFormat)` for each date‑time property.

### `public string? CurrencyFormat`
- **Purpose**: Gets or sets the format string applied to numeric properties deemed as currency (e.g., properties with names containing "Price", "Cost", "Amount", or decorated with a `[DataType(DataType.Currency)]` attribute, depending on implementation).
- **Default value**: `null`, which uses the default `ToString()` representation.
- **Remarks**: When set, the formatter applies `value.ToString(CurrencyFormat)` to matching numeric properties.

### `public List<string>? ColumnsToInclude`
- **Purpose**: Gets or sets an optional list of property names to restrict the CSV output to only those columns.
- **Default value**: `null`, which includes all public readable properties.
- **Remarks**: If both `ColumnsToInclude` and `ColumnsToExclude` are specified, inclusion takes precedence; properties not listed in `ColumnsToInclude` are omitted.

### `public List<string>? ColumnsToExclude`
- **Purpose**: Gets or sets an optional list of property names to omit from the CSV output.
- **Default value**: `null`, which excludes no properties.
- **Remarks**: Ignored when `ColumnsToInclude` is non‑null; otherwise, any property whose name appears in this list is skipped.

## Usage

### Example 1: Exporting a list of objects to a CSV file
```csharp
using System.Collections.Generic;
using System.IO;
using System.Text;

var orders = new List<Order>
{
    new Order { Id = 1, Customer = "Alice", Total = 250.00m, OrderDate = DateTime.Now },
    new Order { Id = 2, Customer = "Bob",   Total = 150.50m, OrderDate = DateTime.Now.AddDays(-1) }
};

var formatter = new CsvFormatter
{
    IncludeHeader = true,
    Delimiter = ",",
    DateFormat = "yyyy-MM-dd",
    CurrencyFormat = "C2"
};

string csv = CsvFormatter.ToCsv(orders);
File.WriteAllText("orders.csv", csv, Encoding.UTF8);
```

### Example 2: Importing CSV data into a list of dictionaries
```csharp
using System.Collections.Generic;

string csv = File.ReadAllText("products.csv", Encoding.UTF8);

var formatter = new CsvFormatter
{
    IncludeHeader = true,   // first line contains column names
    Delimiter = ","
};

List<Dictionary<string, string>> rows = CsvFormatter.ParseCsv(csv);

foreach (var row in rows)
{
    // row["Id"], row["Name"], row["Price"], etc.
    // Process each row as needed
}
```

## Notes
- The static methods (`ToCsv`, `ToCsvBytes`, `ParseCsv`) do not rely on any mutable static state; they are thread‑safe for concurrent calls as long as the supplied arguments are not mutated by other threads during execution.
- Instance properties (`IncludeHeader`, `Delimiter`, `DateFormat`, `CurrencyFormat`, `ColumnsToInclude`, `ColumnsToExclude`) are not thread‑safe when a single `CsvFormatter` instance is shared across threads without external synchronization. It is recommended to either create a new instance per thread or synchronize access.
- If `ColumnsToInclude` is provided, any property not in that list is omitted regardless of `ColumnsToExclude`.
- When `IncludeHeader` is `false`, the parser generates generic column names (`Column0`, `Column1`, …) based on the ordinal position of values in each row; mismatched column counts between rows will cause a `FormatException`.
- Date and currency formatting are applied only to properties whose runtime type matches `DateTime`, `DateTimeOffset`, or numeric types deemed as currency by the formatter’s internal logic; other types are formatted via `ToString()` with no culture‑specific formatting unless the format strings themselves contain culture‑specific specifiers.
- Empty strings and null values are represented as empty fields (two consecutive delimiters) in the output CSV. During parsing, empty fields yield `null` or `string.Empty` in the resulting dictionary according to the implementation’s handling.
