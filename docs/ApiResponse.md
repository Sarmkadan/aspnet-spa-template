# ApiResponse

A generic response container used to standardize API communication in the ASP.NET SPA template. It encapsulates success status, data payloads, error details, and diagnostic information in a structured format suitable for both successful operations and error scenarios.

## API

### Generic Type Parameters
- `T`: The type of data contained in the response when successful.

### Properties
- **`Success`** (bool)
  Indicates whether the operation completed successfully.
  *Read-only.*

- **`Data`** (T?)
  The payload returned on successful operations. `null` when `Success` is `false`.

- **`Message`** (string?)
  A human-readable message describing the result. Typically used for success messages or user-facing error descriptions.

- **`ErrorCode`** (string?)
  A machine-readable error identifier. Used to categorize failures for programmatic handling. Present only when `Success` is `false`.

- **`TraceId`** (string?)
  A unique identifier for tracing the request through logging and monitoring systems. Automatically generated on error responses.

- **`Timestamp`** (DateTime)
  The UTC time when the response was generated. Immutable after construction.

- **`Metadata`** (Dictionary<string, object>?)
  Additional key-value pairs for extensibility. May be `null` if no metadata is attached.

### Static Methods (Generic)
- **`Ok<T>()`** → `ApiResponse<T>`
  Creates a successful response with no data.
  *Returns:* An `ApiResponse<T>` with `Success = true`, `Data = default`, and `Timestamp` set to `DateTime.UtcNow`.

- **`Ok<T>(T data)`** → `ApiResponse<T>`
  Creates a successful response with the provided data.
  *Parameters:*
  - `data` (T): The payload to include.
  *Returns:* An `ApiResponse<T>` with `Success = true`, `Data = data`, and `Timestamp` set to `DateTime.UtcNow`.

- **`Ok()`** → `ApiResponse`
  Creates a successful response with no data and no generic type.
  *Returns:* An `ApiResponse` with `Success = true`, `Data = null`, and `Timestamp` set to `DateTime.UtcNow`.

- **`Error<T>(string message, string? errorCode = null)`** → `ApiResponse<T>`
  Creates an error response with a message and optional error code.
  *Parameters:*
  - `message` (string): The error description.
  - `errorCode` (string?, optional): A machine-readable error identifier. Defaults to `null`.
  *Returns:* An `ApiResponse<T>` with `Success = false`, `Message = message`, `ErrorCode = errorCode`, `TraceId` set to a new GUID string, and `Timestamp` set to `DateTime.UtcNow`.

- **`Error(string message, string? errorCode = null)`** → `ApiResponse`
  Creates an error response with no generic type.
  *Parameters:*
  - `message` (string): The error description.
  - `errorCode` (string?, optional): A machine-readable error identifier. Defaults to `null`.
  *Returns:* An `ApiResponse` with `Success = false`, `Message = message`, `ErrorCode = errorCode`, `TraceId` set to a new GUID string, and `Timestamp` set to `DateTime.UtcNow`.

### Instance Methods (Generic)
- **`WithMetadata(Dictionary<string, object> metadata)`** → `ApiResponse<T>`
  Adds or replaces metadata in the response.
  *Parameters:*
  - `metadata` (Dictionary<string, object>): The metadata to attach.
  *Returns:* A new `ApiResponse<T>` with the same properties except `Metadata` set to the provided dictionary.
  *Note:* The original response is unchanged; this method returns a new instance.

- **`Map<TNew>(Func<T, TNew> mapper)`** → `ApiResponse<TNew>`
  Transforms the data payload using the provided function.
  *Parameters:*
  - `mapper` (Func<T, TNew>): A function to convert `T` to `TNew`.
  *Returns:* A new `ApiResponse<TNew>` with `Data` set to the result of `mapper`, and all other properties copied from the source response.
  *Throws:* `ArgumentNullException` if `mapper` is `null`.

### Non-Generic Type
- **`ApiResponse`**
  A non-generic variant of `ApiResponse<T>` for operations that do not return typed data. Inherits all properties and methods of the generic type except those involving `Data`.

### Properties (Non-Generic)
- **`Success`** (bool)
  Indicates whether the operation completed successfully.
  *Read-only.*

- **`Message`** (string?)
  A human-readable message describing the result.

- **`ErrorCode`** (string?)
  A machine-readable error identifier.

- **`TraceId`** (string?)
  A unique identifier for tracing the request.

- **`Timestamp`** (DateTime)
  The UTC time when the response was generated.

### Static Methods (Non-Generic)
- **`Ok()`** → `ApiResponse`
  Creates a successful response with no data.

- **`Error(string message, string? errorCode = null)`** → `ApiResponse`
  Creates an error response with a message and optional error code.

## Usage

### Example 1: Successful Response with Data
