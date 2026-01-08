// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace AspNetSpaTemplate.Formatters;

/// <summary>
/// Utility for formatting objects as CSV (comma-separated values).
/// Useful for exporting data to Excel or CSV files.
/// Handles escaping of special characters and null values.
/// </summary>
public static class CsvFormatter
{
    /// <summary>
    /// Converts collection of objects to CSV format.
    /// Uses object properties as CSV columns.
    /// </summary>
    public static string ToCsv<T>(IEnumerable<T> items) where T : class
    {
        var sb = new StringBuilder();

        if (!items.Any())
            return string.Empty;

        // Get properties from first item
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Write header
        var headers = properties.Select(p => EscapeCsvValue(p.Name));
        sb.AppendLine(string.Join(",", headers));

        // Write data rows
        foreach (var item in items)
        {
            var values = properties.Select(p => EscapeCsvValue(p.GetValue(item)));
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports collection to CSV bytes (UTF-8 encoded).
    /// Ready for file download.
    /// </summary>
    public static byte[] ToCsvBytes<T>(IEnumerable<T> items) where T : class
    {
        var csv = ToCsv(items);
        return Encoding.UTF8.GetBytes(csv);
    }

    /// <summary>
    /// Escapes CSV value to prevent injection attacks and format errors.
    /// Wraps in quotes if contains comma, quote, or newline.
    /// </summary>
    private static string EscapeCsvValue(object? value)
    {
        if (value == null)
            return string.Empty;

        var strValue = value.ToString() ?? string.Empty;

        // If contains special characters, wrap in quotes and escape inner quotes
        if (strValue.Contains(",") || strValue.Contains("\"") || strValue.Contains("\n"))
        {
            return $"\"{strValue.Replace("\"", "\"\"")}\"";
        }

        return strValue;
    }

    /// <summary>
    /// Parses CSV string back to objects.
    /// Expects header row and simple structure.
    /// </summary>
    public static List<Dictionary<string, string>> ParseCsv(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
            return new List<Dictionary<string, string>>();

        var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (lines.Length < 2)
            return new List<Dictionary<string, string>>();

        // Parse header
        var headers = ParseCsvLine(lines[0]);
        var results = new List<Dictionary<string, string>>();

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var values = ParseCsvLine(lines[i]);
            var row = new Dictionary<string, string>();

            for (int j = 0; j < headers.Count && j < values.Count; j++)
            {
                row[headers[j]] = values[j];
            }

            results.Add(row);
        }

        return results;
    }

    /// <summary>
    /// Parses a single CSV line, handling quoted values.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                // Check for escaped quote
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue = new StringBuilder();
            }
            else
            {
                currentValue.Append(ch);
            }
        }

        // Add final value
        values.Add(currentValue.ToString().Trim());

        return values;
    }
}

/// <summary>
/// CSV export configuration for customizing output.
/// </summary>
public class CsvExportOptions
{
    public bool IncludeHeader { get; set; } = true;
    public string Delimiter { get; set; } = ",";
    public string? DateFormat { get; set; } = "yyyy-MM-dd";
    public string? CurrencyFormat { get; set; } = "F2";
    public List<string>? ColumnsToInclude { get; set; }
    public List<string>? ColumnsToExclude { get; set; }
}
