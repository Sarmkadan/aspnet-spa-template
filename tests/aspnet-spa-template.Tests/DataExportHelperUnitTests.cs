#nullable enable
using AspNetSpaTemplate.Formatters;
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="DataExportHelper"/> class.
/// Tests various export methods including CSV, JSON, and XML formats.
/// </summary>
public sealed class DataExportHelperUnitTests
{
    /// <summary>
    /// Tests that <see cref="DataExportHelper.ExportData{T}(IEnumerable{T}, ExportFormat, string?)"/>
    /// exports data to CSV format correctly.
    /// </summary>
    [Fact]
    public void ExportData_WithCsvFormat_ReturnsCorrectFormat()
    {
        // Arrange
        var items = new List<TestExportItem>
        {
            new TestExportItem { Id = 1, Name = "Item 1", Value = 100.50m },
            new TestExportItem { Id = 2, Name = "Item 2", Value = 200.75m }
        };
        var format = ExportFormat.Csv;

        // Act
        var result = DataExportHelper.ExportData(items, format);

        // Assert
        result.Data.Should().NotBeNullOrEmpty();
        result.ContentType.Should().Be("text/csv");
        result.FileName.Should().Contain("testexportitem-");
        result.FileName.Should().EndWith(".csv");

        // Verify CSV content
        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        csvContent.Should().Contain("Id,Name,Value");
        csvContent.Should().Contain("1,Item 1,100.50");
        csvContent.Should().Contain("2,Item 2,200.75");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.ExportData{T}(IEnumerable{T}, ExportFormat, string?)"/>
    /// exports data to JSON format correctly.
    /// </summary>
    [Fact]
    public void ExportData_WithJsonFormat_ReturnsCorrectFormat()
    {
        // Arrange
        var items = new List<TestExportItem>
        {
            new TestExportItem { Id = 1, Name = "Item 1", Value = 100.50m },
            new TestExportItem { Id = 2, Name = "Item 2", Value = 200.75m }
        };
        var format = ExportFormat.Json;

        // Act
        var result = DataExportHelper.ExportData(items, format);

        // Assert
        result.Data.Should().NotBeNullOrEmpty();
        result.ContentType.Should().Be("application/json");
        result.FileName.Should().Contain("testexportitem-");
        result.FileName.Should().EndWith(".json");

        // Verify JSON content is valid
        var jsonContent = System.Text.Encoding.UTF8.GetString(result.Data);
        jsonContent.Should().NotBeNullOrEmpty();
        jsonContent.Should().Contain("Item 1");
        jsonContent.Should().Contain("Item 2");
    }


    /// <summary>
    /// Tests that <see cref="DataExportHelper.ExportData{T}(IEnumerable{T}, ExportFormat, string?)"/>
    /// uses custom file name when provided.
    /// </summary>
    [Fact]
    public void ExportData_WithCustomFileName_UsesCustomName()
    {
        // Arrange
        var items = new List<TestExportItem> { new TestExportItem { Id = 1, Name = "Test" } };
        var format = ExportFormat.Json;
        var customFileName = "custom-export";

        // Act
        var result = DataExportHelper.ExportData(items, format, customFileName);

        // Assert
        result.FileName.Should().StartWith(customFileName);
        result.FileName.Should().EndWith(".json");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.ExportData{T}(IEnumerable{T}, ExportFormat, string?)"/>
    /// throws exception for unsupported format.
    /// </summary>
    [Fact]
    public void ExportData_WithUnsupportedFormat_ThrowsArgumentException()
    {
        // Arrange
        var items = new List<TestExportItem> { new TestExportItem { Id = 1, Name = "Test" } };
        var invalidFormat = (ExportFormat)999; // Invalid enum value

        // Act
        var act = () => DataExportHelper.ExportData(items, invalidFormat);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.ExportData{T}(IEnumerable{T}, ExportFormat, string?)"/>
    /// handles empty collection correctly.
    /// </summary>
    [Fact]
    public void ExportData_WithEmptyCollection_HandlesGracefully()
    {
        // Arrange
        var items = new List<TestExportItem>();
        var format = ExportFormat.Csv;

        // Act
        var result = DataExportHelper.ExportData(items, format);

        // Assert
        result.Data.Should().BeEmpty();
        result.ContentType.Should().Be("text/csv");
        result.FileName.Should().Contain("testexportitem-");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.ExportData{T}(IEnumerable{T}, ExportFormat, string?)"/>
    /// handles collection with null values correctly.
    /// </summary>
    [Fact]
    public void ExportData_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var items = new List<TestExportItem> {
            new TestExportItem { Id = 1, Name = "Item 1", Value = 100.50m },
            new TestExportItem { Id = 2, Name = null, Value = 200.75m }
        };
        var format = ExportFormat.Csv;

        // Act
        var result = DataExportHelper.ExportData(items, format);

        // Assert
        result.Data.Should().NotBeNullOrEmpty();
        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        csvContent.Should().Contain("Id,Name,Value");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Json as default when accept header is null.
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithNullAcceptHeader_ReturnsJson()
    {
        // Arrange
        string? acceptHeader = null;

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Json);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Json as default when accept header is empty.
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithEmptyAcceptHeader_ReturnsJson()
    {
        // Arrange
        var acceptHeader = string.Empty;

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Json);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Csv when accept header is "text/csv".
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithCsvAcceptHeader_ReturnsCsv()
    {
        // Arrange
        var acceptHeader = "text/csv";

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Csv);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Csv when accept header is "application/csv".
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithApplicationCsvAcceptHeader_ReturnsCsv()
    {
        // Arrange
        var acceptHeader = "application/csv";

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Csv);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Xml when accept header is "application/xml".
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithXmlAcceptHeader_ReturnsXml()
    {
        // Arrange
        var acceptHeader = "application/xml";

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Xml);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Xml when accept header is "text/xml".
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithTextXmlAcceptHeader_ReturnsXml()
    {
        // Arrange
        var acceptHeader = "text/xml";

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Xml);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Json when accept header is "application/json".
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithJsonAcceptHeader_ReturnsJson()
    {
        // Arrange
        var acceptHeader = "application/json";

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Json);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.NegotiateFormat(string?)"/>
    /// returns Json as fallback for unsupported accept header.
    /// </summary>
    [Fact]
    public void NegotiateFormat_WithUnsupportedAcceptHeader_ReturnsJson()
    {
        // Arrange
        var acceptHeader = "text/html";

        // Act
        var result = DataExportHelper.NegotiateFormat(acceptHeader);

        // Assert
        result.Should().Be(ExportFormat.Json);
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetFileExtension(ExportFormat)"/>
    /// returns correct extension for Csv format.
    /// </summary>
    [Fact]
    public void GetFileExtension_WithCsvFormat_ReturnsCsvExtension()
    {
        // Act
        var result = DataExportHelper.GetFileExtension(ExportFormat.Csv);

        // Assert
        result.Should().Be(".csv");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetFileExtension(ExportFormat)"/>
    /// returns correct extension for Json format.
    /// </summary>
    [Fact]
    public void GetFileExtension_WithJsonFormat_ReturnsJsonExtension()
    {
        // Act
        var result = DataExportHelper.GetFileExtension(ExportFormat.Json);

        // Assert
        result.Should().Be(".json");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetFileExtension(ExportFormat)"/>
    /// returns correct extension for Xml format.
    /// </summary>
    [Fact]
    public void GetFileExtension_WithXmlFormat_ReturnsXmlExtension()
    {
        // Act
        var result = DataExportHelper.GetFileExtension(ExportFormat.Xml);

        // Assert
        result.Should().Be(".xml");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetFileExtension(ExportFormat)"/>
    /// returns default extension for unknown format.
    /// </summary>
    [Fact]
    public void GetFileExtension_WithUnknownFormat_ReturnsDefaultExtension()
    {
        // Arrange
        var unknownFormat = (ExportFormat)999;

        // Act
        var result = DataExportHelper.GetFileExtension(unknownFormat);

        // Assert
        result.Should().Be(".dat");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetContentType(ExportFormat)"/>
    /// returns correct MIME type for Csv format.
    /// </summary>
    [Fact]
    public void GetContentType_WithCsvFormat_ReturnsCsvContentType()
    {
        // Act
        var result = DataExportHelper.GetContentType(ExportFormat.Csv);

        // Assert
        result.Should().Be("text/csv");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetContentType(ExportFormat)"/>
    /// returns correct MIME type for Json format.
    /// </summary>
    [Fact]
    public void GetContentType_WithJsonFormat_ReturnsJsonContentType()
    {
        // Act
        var result = DataExportHelper.GetContentType(ExportFormat.Json);

        // Assert
        result.Should().Be("application/json");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetContentType(ExportFormat)"/>
    /// returns correct MIME type for Xml format.
    /// </summary>
    [Fact]
    public void GetContentType_WithXmlFormat_ReturnsXmlContentType()
    {
        // Act
        var result = DataExportHelper.GetContentType(ExportFormat.Xml);

        // Assert
        result.Should().Be("application/xml");
    }

    /// <summary>
    /// Tests that <see cref="DataExportHelper.GetContentType(ExportFormat)"/>
    /// returns default MIME type for unknown format.
    /// </summary>
    [Fact]
    public void GetContentType_WithUnknownFormat_ReturnsDefaultContentType()
    {
        // Arrange
        var unknownFormat = (ExportFormat)999;

        // Act
        var result = DataExportHelper.GetContentType(unknownFormat);

        // Assert
        result.Should().Be("application/octet-stream");
    }

    /// <summary>
    /// Test data model for export testing.
    /// </summary>
    private class TestExportItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
    }
}
