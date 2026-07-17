# DataExportHelper

The `DataExportHelper` class provides a centralized utility for exporting generic collections of data into various file formats suitable for browser download. It handles format negotiation, content type resolution, optional compression, and the construction of `FileContentResult` objects for ASP.NET Core MVC controllers. The helper supports both static convenience methods for one-off exports and an instance-based configuration approach for customized export pipelines.

## API

### Static Methods

#### `ExportData<T>`
Generates the raw binary data, content type, and suggested file name for a given collection.
*   **Signature**: `public static (byte[] Data, string ContentType, string FileName) ExportData<T>(IEnumerable<T> data, ExportFormat format, string? fileNamePrefix = null, bool includeTimestamp = true, bool includeHeaders = true, List<string>? columnsToInclude = null, List<string>? columnsToExclude = null, bool compress = false)`
*   **Purpose**: Serializes the input `data` into the specified `format`, applying column filtering and compression settings.
*   **Parameters**:
    *   `data`: The collection of objects to export.
    *   `format`: The target export format (e.g., CSV, JSON, Excel).
    *   `fileNamePrefix`: Optional prefix for the generated file name.
    *   `includeTimestamp`: If true, appends a UTC timestamp to the file name.
    *   `includeHeaders`: If true, includes property names as the first row/keys.
    *   `columnsToInclude`: Whitelist of property names to export. If null, all public properties are considered.
    *   `columnsToExclude`: Blacklist of property names to omit.
    *   `compress`: If true, wraps the output in GZip compression.
*   **Return Value**: A tuple containing the byte array (`Data`), the MIME type (`ContentType`), and the suggested `FileName`.
*   **Exceptions**: Throws `ArgumentNullException` if `data` is null. Throws `InvalidOperationException` if the specified `format` is unsupported or if conflicting column lists are provided.

#### `NegotiateFormat`
Determines the appropriate export format based on the HTTP Accept header.
*   **Signature**: `public static ExportFormat NegotiateFormat(string? acceptHeader)`
*   **Purpose**: Parses the `acceptHeader` string to select the best matching `ExportFormat`.
*   **Parameters**: `acceptHeader`: The value of the HTTP `Accept` header.
*   **Return Value**: An `ExportFormat` enum value. Defaults to a standard format (typically CSV or JSON) if no match is found or the header is null.
*   **Exceptions**: None.

#### `GetFileExtension`
Retrieves the standard file extension for a specific format.
*   **Signature**: `public static string GetFileExtension(ExportFormat format)`
*   **Purpose**: Returns the dot-prefixed extension (e.g., `.csv`, `.json`) associated with the format.
*   **Parameters**: `format`: The export format.
*   **Return Value**: A string representing the file extension.
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if the format is unrecognized.

#### `GetContentType`
Retrieves the MIME type for a specific format.
*   **Signature**: `public static string GetContentType(ExportFormat format)`
*   **Purpose**: Returns the correct HTTP Content-Type header value for the format.
*   **Parameters**: `format`: The export format.
*   **Return Value**: A string representing the MIME type (e.g., `text/csv`, `application/json`).
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if the format is unrecognized.

#### `ToFileContent`
Converts raw export data directly into an ASP.NET Core `FileContentResult`.
*   **Signature**: `public static FileContentResult ToFileContent(byte[] data, string contentType, string fileName)`
*   **Purpose**: Wraps the binary data and metadata into a result object ready for returning from a controller action.
*   **Parameters**: `data`, `contentType`, `fileName`: The components typically returned by `ExportData`.
*   **Return Value**: A `FileContentResult` instance.
*   **Exceptions**: Throws `ArgumentNullException` if any parameter is null.

#### `ExportAsFile<T>`
A high-level convenience method that combines data export and result creation.
*   **Signature**: `public static FileContentResult ExportAsFile<T>(IEnumerable<T> data, ExportFormat format, string? fileNamePrefix = null, ...)`
*   **Purpose**: Executes the full pipeline: serializes `data`, determines headers, and returns a `FileContentResult` immediately. Parameters mirror `ExportData<T>`.
*   **Return Value**: A `FileContentResult` instance.
*   **Exceptions**: Propagates exceptions from `ExportData<T>`.

### Instance Members

The following members belong to an instance of `DataExportHelper` used for configuring stateful export operations.

#### `Format`
*   **Signature**: `public ExportFormat Format { get; set; }`
*   **Purpose**: Gets or sets the default export format for this instance.

#### `IncludeHeaders`
*   **Signature**: `public bool IncludeHeaders { get; set; }`
*   **Purpose**: Gets or sets whether property names should be included as headers in the output.

#### `ColumnsToInclude`
*   **Signature**: `public List<string>? ColumnsToInclude { get; set; }`
*   **Purpose**: Gets or sets the whitelist of columns to export. If set, only these properties are serialized.

#### `ColumnsToExclude`
*   **Signature**: `public List<string>? ColumnsToExclude { get; set; }`
*   **Purpose**: Gets or sets the blacklist of columns to omit from serialization.

#### `Compress`
*   **Signature**: `public bool Compress { get; set; }`
*   **Purpose**: Gets or sets whether the output stream should be GZip compressed.

#### `FileNamePrefix`
*   **Signature**: `public string? FileNamePrefix { get; set; }`
*   **Purpose**: Gets or sets the prefix applied to the generated file name.

#### `IncludeTimestamp`
*   **Signature**: `public bool IncludeTimestamp { get; set; }`
*   **Purpose**: Gets or sets whether a UTC timestamp is appended to the file name.

## Usage

### Example 1: Static One-Off Export
This example demonstrates exporting a list of user records to CSV directly from a controller action, including a timestamp in the filename and excluding sensitive fields.

```csharp
using Microsoft.AspNetCore.Mvc;
using AspnetSpaTemplate.Helpers;

public class UsersController : Controller
{
    public IActionResult DownloadUsers()
    {
        var users = _userService.GetAllUsers();
        
        // Define columns to exclude (e.g., PasswordHash, InternalId)
        var excludeColumns = new List<string> { "PasswordHash", "InternalId" };

        // Perform export and return file immediately
        return DataExportHelper.ExportAsFile(
            data: users,
            format: ExportFormat.Csv,
            fileNamePrefix: "users_report",
            includeTimestamp: true,
            includeHeaders: true,
            columnsToExclude: excludeColumns,
            compress: false
        );
    }
}
```

### Example 2: Configured Instance with Format Negotiation
This example shows how to use the helper's instance configuration alongside format negotiation based on the client's `Accept` header, allowing the same endpoint to serve JSON or Excel.

```csharp
using Microsoft.AspNetCore.Mvc;
using AspnetSpaTemplate.Helpers;

public class ReportsController : Controller
{
    public IActionResult GetReport()
    {
        var data = _reportService.GetMonthlyData();
        
        // Negotiate format based on request headers
        var acceptHeader = Request.Headers["Accept"].FirstOrDefault();
        var format = DataExportHelper.NegotiateFormat(acceptHeader);

        // Configure the helper instance
        var exporter = new DataExportHelper
        {
            Format = format,
            FileNamePrefix = "monthly_report",
            IncludeTimestamp = true,
            IncludeHeaders = true,
            Compress = format == ExportFormat.Json, // Compress JSON by default
            ColumnsToInclude = new List<string> { "Date", "Revenue", "UnitsSold" }
        };

        // Generate raw data tuple
        var (dataBytes, contentType, fileName) = DataExportHelper.ExportData(
            data: data,
            format: exporter.Format,
            fileNamePrefix: exporter.FileNamePrefix,
            includeTimestamp: exporter.IncludeTimestamp,
            includeHeaders: exporter.IncludeHeaders,
            columnsToInclude: exporter.ColumnsToInclude,
            columnsToExclude: exporter.ColumnsToExclude,
            compress: exporter.Compress
        );

        // Return as file result
        return DataExportHelper.ToFileContent(dataBytes, contentType, fileName);
    }
}
```

## Notes

*   **Column Filtering Logic**: If both `ColumnsToInclude` and `ColumnsToExclude` are provided, `ColumnsToInclude` takes precedence. If `ColumnsToInclude` is null, the exporter processes all public readable properties minus those listed in `ColumnsToExclude`.
*   **Compression Behavior**: When `Compress` is set to `true`, the `ContentType` returned by `ExportData` or `GetContentType` may be adjusted to indicate compressed content (e.g., `application/gzip`), and the file extension will be appended with `.gz` where appropriate.
*   **Thread Safety**: The static methods (`ExportData`, `ExportAsFile`, etc.) are thread-safe as they do not maintain internal state between calls. However, instances of `DataExportHelper` are **not** thread-safe. Do not share a single configured instance across multiple concurrent requests; instantiate a new helper per request or scope.
*   **Memory Usage**: The `ExportData` method loads the entire serialized dataset into a `byte[]` in memory before returning. For extremely large datasets (millions of rows), consider streaming alternatives or pagination to prevent `OutOfMemoryException`.
*   **Timestamp Format**: The `IncludeTimestamp` feature uses UTC time formatted as `yyyyMMdd_HHmmss` to ensure file system compatibility across different operating systems.
