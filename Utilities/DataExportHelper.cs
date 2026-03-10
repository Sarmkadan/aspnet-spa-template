// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Formatters;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Helper for exporting data in multiple formats (CSV, JSON, XML).
/// Supports format negotiation and custom field selection.
/// </summary>
public static class DataExportHelper
{
    /// <summary>
    /// Exports data to requested format.
    /// </summary>
    public static (byte[] Data, string ContentType, string FileName) ExportData<T>(
        IEnumerable<T> items,
        ExportFormat format,
        string? fileName = null) where T : class
    {
        fileName ??= typeof(T).Name.ToLowerInvariant();

        return format switch
        {
            ExportFormat.Csv => ExportToCsv(items, fileName),
            ExportFormat.Json => ExportToJson(items, fileName),
            ExportFormat.Xml => ExportToXml(items, fileName),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    /// <summary>
    /// Exports to CSV format.
    /// </summary>
    private static (byte[] Data, string ContentType, string FileName) ExportToCsv<T>(
        IEnumerable<T> items,
        string fileName) where T : class
    {
        var csv = CsvFormatter.ToCsv(items);
        var data = System.Text.Encoding.UTF8.GetBytes(csv);
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fullFileName = $"{fileName}-{timestamp}.csv";

        return (data, "text/csv", fullFileName);
    }

    /// <summary>
    /// Exports to JSON format.
    /// </summary>
    private static (byte[] Data, string ContentType, string FileName) ExportToJson<T>(
        IEnumerable<T> items,
        string fileName) where T : class
    {
        var json = JsonSerializationHelper.SerializePretty(items.ToList());
        var data = System.Text.Encoding.UTF8.GetBytes(json);
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fullFileName = $"{fileName}-{timestamp}.json";

        return (data, "application/json", fullFileName);
    }

    /// <summary>
    /// Exports to XML format.
    /// </summary>
    private static (byte[] Data, string ContentType, string FileName) ExportToXml<T>(
        IEnumerable<T> items,
        string fileName) where T : class
    {
        var wrapper = new { Items = items.ToList() };
        var xml = XmlFormatter.ToXml(wrapper);
        var data = System.Text.Encoding.UTF8.GetBytes(xml);
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fullFileName = $"{fileName}-{timestamp}.xml";

        return (data, "application/xml", fullFileName);
    }

    /// <summary>
    /// Negotiates export format from request headers.
    /// Supports Accept header for content negotiation.
    /// </summary>
    public static ExportFormat NegotiateFormat(string? acceptHeader)
    {
        if (string.IsNullOrEmpty(acceptHeader))
            return ExportFormat.Json;

        return acceptHeader.ToLowerInvariant() switch
        {
            "text/csv" or "application/csv" => ExportFormat.Csv,
            "application/xml" or "text/xml" => ExportFormat.Xml,
            "application/json" => ExportFormat.Json,
            _ => ExportFormat.Json
        };
    }

    /// <summary>
    /// Gets file extension for export format.
    /// </summary>
    public static string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => ".csv",
            ExportFormat.Json => ".json",
            ExportFormat.Xml => ".xml",
            _ => ".dat"
        };
    }

    /// <summary>
    /// Gets MIME type for export format.
    /// </summary>
    public static string GetContentType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => "text/csv",
            ExportFormat.Json => "application/json",
            ExportFormat.Xml => "application/xml",
            _ => "application/octet-stream"
        };
    }
}

public enum ExportFormat
{
    Json,
    Csv,
    Xml
}

/// <summary>
/// Export options for customizing output.
/// </summary>
public class DataExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.Json;
    public bool IncludeHeaders { get; set; } = true;
    public List<string>? ColumnsToInclude { get; set; }
    public List<string>? ColumnsToExclude { get; set; }
    public bool Compress { get; set; } = false;
    public string? FileNamePrefix { get; set; }
    public bool IncludeTimestamp { get; set; } = true;
}

/// <summary>
/// Extension methods for exporting from HTTP context.
/// </summary>
public static class ExportExtensions
{
    /// <summary>
    /// Creates file response for download.
    /// </summary>
    public static FileContentResult ToFileContent(
        this (byte[] Data, string ContentType, string FileName) export)
    {
        return new FileContentResult(export.Data, export.ContentType)
        {
            FileDownloadName = export.FileName
        };
    }

    /// <summary>
    /// Negotiates and exports data in single call.
    /// </summary>
    public static FileContentResult ExportAsFile<T>(
        this IEnumerable<T> items,
        HttpContext context,
        string? fileName = null) where T : class
    {
        var format = DataExportHelper.NegotiateFormat(context.Request.Headers["Accept"].ToString());
        var export = DataExportHelper.ExportData(items, format, fileName);
        return export.ToFileContent();
    }
}
