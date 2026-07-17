# XmlFormatter
The `XmlFormatter` type is designed to facilitate the serialization and deserialization of .NET objects to and from XML. It provides a range of methods and properties to control the formatting and parsing of XML data, allowing for customization of the output and input processes. This enables developers to easily convert objects into XML strings or byte arrays and vice versa, making it a useful utility in various application scenarios, especially those involving data exchange or storage in XML format.

## API
### Static Methods
- `ToXml<T>(T obj)`: Converts an object of type `T` into an XML string. The object is serialized into XML, and the resulting string is returned. This method throws if the object cannot be serialized.
- `ToXmlBytes<T>(T obj)`: Similar to `ToXml<T>`, but returns the XML data as a byte array instead of a string.
- `FromXml<T>(string xml)`: Deserializes an XML string into an object of type `T`. Returns `null` if the deserialization fails.
- `FromXmlSafe<T>(string xml)`: A safer version of `FromXml<T>`, designed to handle potential exceptions during deserialization more gracefully. Returns `null` if deserialization fails.
### Instance Properties
- `Indent`: A boolean indicating whether the XML output should be indented for readability.
- `OmitXmlDeclaration`: A boolean that determines whether the XML declaration should be omitted from the output.
- `IndentChars`: A string specifying the characters to use for indentation in the XML output.
- `RootElementName`: The name of the root element in the generated XML.
- `ItemElementName`: The name of the element used to represent items in collections within the XML.
- `Encoding`: The text encoding to use when converting XML to or from byte arrays.

## Usage
The following examples demonstrate how to use the `XmlFormatter` to serialize and deserialize objects:
```csharp
// Example 1: Serializing an object to XML
var person = new Person { Name = "John Doe", Age = 30 };
var xml = XmlFormatter.ToXml(person);
Console.WriteLine(xml);

// Example 2: Deserializing XML back to an object
var xmlString = "<Person><Name>John Doe</Name><Age>30</Age></Person>";
var deserializedPerson = XmlFormatter.FromXml<Person>(xmlString);
Console.WriteLine(deserializedPerson.Name); // Outputs: John Doe
```

## Notes
- **Thread Safety**: The static methods of `XmlFormatter` are thread-safe since they do not rely on any instance state. However, instance properties (`Indent`, `OmitXmlDeclaration`, etc.) are not thread-safe if accessed concurrently from multiple threads.
- **Edge Cases**: When using `FromXml<T>` or `FromXmlSafe<T>`, be aware that the XML string must conform to the structure expected by the type `T`, or deserialization will fail. Additionally, the `ToXml<T>` and `ToXmlBytes<T>` methods will throw exceptions if the object being serialized contains circular references or if it cannot be serialized for other reasons.
- **Customization**: The properties like `IndentChars`, `RootElementName`, and `ItemElementName` allow for customization of the XML output. However, modifying these properties does not affect the deserialization process, which relies on the structure of the XML input rather than these settings.
