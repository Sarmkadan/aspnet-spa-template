// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml;
using System.Xml.Serialization;

namespace AspNetSpaTemplate.Formatters;

/// <summary>
/// Utility for formatting objects as XML.
/// Useful for legacy system integrations or API responses in XML format.
/// Handles serialization with proper encoding and formatting options.
/// </summary>
public static class XmlFormatter
{
    /// <summary>
    /// Serializes object to XML string.
    /// </summary>
    public static string ToXml<T>(T obj) where T : class
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = System.Text.Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using (var stream = new StringWriter())
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, obj);
            return stream.ToString();
        }
    }

    /// <summary>
    /// Serializes object to XML bytes (UTF-8 encoded).
    /// Ready for file download or transmission.
    /// </summary>
    public static byte[] ToXmlBytes<T>(T obj) where T : class
    {
        var xml = ToXml(obj);
        return System.Text.Encoding.UTF8.GetBytes(xml);
    }

    /// <summary>
    /// Deserializes XML string back to object.
    /// Throws XmlException if format is invalid.
    /// </summary>
    public static T? FromXml<T>(string xml) where T : class
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return serializer.Deserialize(reader) as T;
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new XmlException($"Failed to deserialize XML to {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Safely deserializes XML (returns null on error, doesn't throw).
    /// </summary>
    public static T? FromXmlSafe<T>(string? xml) where T : class
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            return FromXml<T>(xml);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates XML format without deserializing.
    /// Useful for rejecting malformed payloads early.
    /// </summary>
    public static bool IsValidXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts specific value from XML using XPath.
    /// Useful for retrieving nested values without full deserialization.
    /// </summary>
    public static string? GetXmlValue(string xml, string xpath)
    {
        if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(xpath))
            return null;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var node = doc.SelectSingleNode(xpath);
            return node?.InnerText;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates XML element with attributes.
    /// Builder pattern for constructing XML programmatically.
    /// </summary>
    public static XmlElement CreateElement(string name, Dictionary<string, string> attributes = null!)
    {
        var doc = new XmlDocument();
        var element = doc.CreateElement(name);

        if (attributes != null)
        {
            foreach (var attr in attributes)
            {
                element.SetAttribute(attr.Key, attr.Value);
            }
        }

        return element;
    }
}

/// <summary>
/// XML export configuration for customizing output.
/// </summary>
public class XmlExportOptions
{
    public bool Indent { get; set; } = true;
    public bool OmitXmlDeclaration { get; set; } = false;
    public string IndentChars { get; set; } = "  ";
    public string RootElementName { get; set; } = "Data";
    public string ItemElementName { get; set; } = "Item";
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
}
