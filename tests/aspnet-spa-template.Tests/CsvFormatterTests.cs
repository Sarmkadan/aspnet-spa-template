using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspNetSpaTemplate.Formatters;
using Xunit;

namespace AspNetSpaTemplate.Tests
{
    public class CsvFormatterTests
    {
        private sealed class TestItem
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        [Fact]
        public void ToCsv_EscapesSpecialCharactersCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem
                {
                    Name = "Item,1",
                    Description = "Description \"with\" quotes"
                },
                new TestItem
                {
                    Name = "Item\n2",
                    Description = "Line1\nLine2"
                }
            };

            // Act
            var csv = CsvFormatter.ToCsv(items);

            // Assert
            // Header
            Assert.StartsWith("Name,Description", csv);

            // First row
            var expectedFirstRow = "\"Item,1\",\"Description \"\"with\"\" quotes\"";
            Assert.Contains(expectedFirstRow, csv);

            // Second row
            var expectedSecondRow = "\"Item\n2\",\"Line1\nLine2\"";
            Assert.Contains(expectedSecondRow, csv);
        }

        [Fact]
        public void ToCsv_EmptyInput_ReturnsEmptyString()
        {
            // Arrange
            var items = Enumerable.Empty<TestItem>();

            // Act
            var csv = CsvFormatter.ToCsv(items);

            // Assert
            Assert.Equal(string.Empty, csv);
        }

        [Fact]
        public void ToCsv_IncludesHeaderRowByDefault()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "A", Description = "B" }
            };

            // Act
            var csv = CsvFormatter.ToCsv(items);

            // Assert
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Assert.NotEmpty(lines);
            Assert.Equal("Name,Description", lines[0]);
        }

        [Fact]
        public void ToCsvBytes_ReturnsUtf8EncodedBytes()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Test", Description = "Desc" }
            };

            // Act
            var bytes = CsvFormatter.ToCsvBytes(items);
            var decoded = Encoding.UTF8.GetString(bytes);

            // Assert
            var expectedCsv = "Name,Description\r\nTest,Desc\r\n";
            Assert.Equal(expectedCsv, decoded);
        }
    }
}
