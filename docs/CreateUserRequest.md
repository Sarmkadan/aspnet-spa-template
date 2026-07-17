# CreateUserRequest

Represents a request to create a new user in the system. This type encapsulates the required and optional fields needed to register a user, including personal identification, contact details, and authentication credentials. It is typically used as the input model for user registration endpoints in the ASP.NET Core API.

## API

### `FirstName`
- **Purpose**: The given name of the user.
- **Type**: `string`
- **Constraints**: Required. Must not be null or whitespace.
- **Throws**: `ArgumentException` if validation fails during model binding.

### `LastName`
- **Purpose**: The family name of the user.
- **Type**: `string`
- **Constraints**: Required. Must not be null or whitespace.
- **Throws**: `ArgumentException` if validation fails during model binding.

### `Email`
- **Purpose**: The email address of the user, used for authentication and communication.
- **Type**: `string`
- **Constraints**: Required. Must be a valid email format.
- **Throws**: `ArgumentException` if validation fails during model binding.

### `Password`
- **Purpose**: The plaintext password for the user account, to be hashed before storage.
- **Type**: `string`
- **Constraints**: Required. Must meet minimum complexity requirements (e.g., length, special characters).
- **Throws**: `ArgumentException` if validation fails during model binding.

### `PhoneNumber`
- **Purpose**: The contact phone number of the user.
- **Type**: `string?`
- **Constraints**: Optional. If provided, should be a valid phone number format.
- **Throws**: None.

### `Address`
- **Purpose**: The street address of the user.
- **Type**: `string?`
- **Constraints**: Optional.
- **Throws**: None.

### `City`
- **Purpose**: The city of residence for the user.
- **Type**: `string?`
- **Constraints**: Optional.
- **Throws**: None.

### `PostalCode`
- **Purpose**: The postal code for the user's address.
- **Type**: `string?`
- **Constraints**: Optional. If provided, should conform to regional postal code standards.
- **Throws**: None.

### `Country`
- **Purpose**: The country of residence for the user.
- **Type**: `string?`
- **Constraints**: Optional.
- **Throws**: None.

## Usage

### Example 1: Basic User Registration
