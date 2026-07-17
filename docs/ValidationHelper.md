# ValidationHelper
The `ValidationHelper` class provides a set of static methods for validating various types of data, including strings, collections, and objects. These methods can be used to ensure that data conforms to certain rules or patterns, such as being non-null, non-empty, or matching a specific format. By using these methods, developers can write more robust and reliable code that is less prone to errors and exceptions.

## API
The `ValidationHelper` class includes the following public members:
* `NotNull`: Throws an exception if the specified object is null.
	+ Parameters: The object to check.
	+ Return value: None.
	+ Throws: An exception if the object is null.
* `NotNullOrEmpty`: Throws an exception if the specified string is null or empty.
	+ Parameters: The string to check.
	+ Return value: None.
	+ Throws: An exception if the string is null or empty.
* `InRange`: Throws an exception if the specified value is not within the specified range.
	+ Parameters: The value to check, the minimum value, and the maximum value.
	+ Return value: None.
	+ Throws: An exception if the value is not within the range.
* `LengthBetween`: Throws an exception if the length of the specified string is not within the specified range.
	+ Parameters: The string to check, the minimum length, and the maximum length.
	+ Return value: None.
	+ Throws: An exception if the length is not within the range.
* `MatchesPattern`: Throws an exception if the specified string does not match the specified pattern.
	+ Parameters: The string to check and the pattern to match.
	+ Return value: None.
	+ Throws: An exception if the string does not match the pattern.
* `ValidEmail`: Throws an exception if the specified string is not a valid email address.
	+ Parameters: The string to check.
	+ Return value: None.
	+ Throws: An exception if the string is not a valid email address.
* `ValidPhoneNumber`: Throws an exception if the specified string is not a valid phone number.
	+ Parameters: The string to check.
	+ Return value: None.
	+ Throws: An exception if the string is not a valid phone number.
* `NotEmpty<T>`: Throws an exception if the specified collection is empty.
	+ Parameters: The collection to check.
	+ Return value: None.
	+ Throws: An exception if the collection is empty.
* `MaxItems<T>`: Throws an exception if the specified collection has more items than the specified maximum.
	+ Parameters: The collection to check and the maximum number of items.
	+ Return value: None.
	+ Throws: An exception if the collection has more items than the maximum.
* `Equal<T>`: Throws an exception if the two specified objects are not equal.
	+ Parameters: The two objects to compare.
	+ Return value: None.
	+ Throws: An exception if the objects are not equal.
* `ValidateAndCollectErrors`: Validates the specified data and returns a dictionary of error messages.
	+ Parameters: The data to validate.
	+ Return value: A dictionary of error messages.
	+ Throws: None.

## Usage
Here are two examples of using the `ValidationHelper` class:
```csharp
// Example 1: Validating a string
string email = "user@example.com";
ValidationHelper.ValidEmail(email);

// Example 2: Validating a collection
List<string> items = new List<string> { "item1", "item2" };
ValidationHelper.NotEmpty(items);
ValidationHelper.MaxItems(items, 5);
```
## Notes
When using the `ValidationHelper` class, note that the methods will throw exceptions if the data is invalid. It is recommended to handle these exceptions and provide meaningful error messages to the user. Additionally, the `ValidateAndCollectErrors` method can be used to validate data and collect error messages without throwing exceptions. The `ValidationHelper` class is thread-safe, as all methods are static and do not rely on any instance state. However, the methods may still throw exceptions if the data is invalid, so it is still important to handle these exceptions properly.
