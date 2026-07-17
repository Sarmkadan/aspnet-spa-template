# User

Represents an application user with identity, profile, contact, and activity-tracking properties. This entity serves as the central domain model for authentication, personalization, and relational data such as orders and reviews. It also exposes convenience methods for deriving a display name, validating the email format, and recording login timestamps.

## API

### `public int Id`

Unique identifier for the user. Serves as the primary key in the underlying data store.

- **Value**: An integer assigned upon persistence.
- **Access**: Read/write.

---

### `public string FirstName`

The user’s given name.

- **Value**: A non-null string.
- **Access**: Read/write.

---

### `public string LastName`

The user’s family name.

- **Value**: A non-null string.
- **Access**: Read/write.

---

### `public string Email`

The email address used for login and notifications.

- **Value**: A non-null string expected to contain a valid email format.
- **Access**: Read/write.

---

### `public string PasswordHash`

The hashed representation of the user’s password.

- **Value**: A non-null string produced by the configured hashing algorithm.
- **Access**: Read/write. Should never be set to a plain-text password.

---

### `public string? PhoneNumber`

The user’s contact phone number, if provided.

- **Value**: Nullable string.
- **Access**: Read/write.

---

### `public string? Address`

The street address for shipping or billing, if provided.

- **Value**: Nullable string.
- **Access**: Read/write.

---

### `public string? City`

The city portion of the user’s address, if provided.

- **Value**: Nullable string.
- **Access**: Read/write.

---

### `public string? PostalCode`

The postal or ZIP code, if provided.

- **Value**: Nullable string.
- **Access**: Read/write.

---

### `public string? Country`

The country name or code, if provided.

- **Value**: Nullable string.
- **Access**: Read/write.

---

### `public bool IsActive`

Indicates whether the user account is active. Inactive accounts are typically prevented from authenticating.

- **Value**: `true` if the account is active; otherwise `false`.
- **Access**: Read/write.

---

### `public bool IsEmailVerified`

Indicates whether the user has confirmed ownership of the email address.

- **Value**: `true` if the email has been verified; otherwise `false`.
- **Access**: Read/write.

---

### `public DateTime CreatedAt`

The UTC timestamp when the user record was first persisted.

- **Value**: A `DateTime` value, typically set once during creation.
- **Access**: Read/write.

---

### `public DateTime? UpdatedAt`

The UTC timestamp of the most recent modification to the user record, if any.

- **Value**: Nullable `DateTime`. Null when the record has never been updated.
- **Access**: Read/write.

---

### `public DateTime? LastLoginAt`

The UTC timestamp of the user’s most recent successful authentication, if any.

- **Value**: Nullable `DateTime`. Null when the user has never logged in.
- **Access**: Read/write.

---

### `public ICollection<Order>? Orders`

The collection of orders placed by the user.

- **Value**: Nullable collection of `Order` entities. May be null if the navigation property is not loaded.
- **Access**: Read/write.

---

### `public ICollection<Review>? Reviews`

The collection of product reviews submitted by the user.

- **Value**: Nullable collection of `Review` entities. May be null if the navigation property is not loaded.
- **Access**: Read/write.

---

### `public string GetFullName`

Returns the concatenation of `FirstName` and `LastName`, separated by a single space.

- **Parameters**: None.
- **Returns**: A string in the format `"{FirstName} {LastName}"`.
- **Exceptions**: None thrown directly. If either property is null, the resulting string will contain the null segment; this is a caller-side concern.

---

### `public bool IsValidEmail`

Performs a basic format check on the `Email` property.

- **Parameters**: None.
- **Returns**: `true` if `Email` matches a typical email pattern; otherwise `false`.
- **Exceptions**: None thrown. Does not verify deliverability or domain existence.

---

### `public void UpdateLastLogin()`

Sets `LastLoginAt` to the current UTC date and time.

- **Parameters**: None.
- **Returns**: Void.
- **Exceptions**: None thrown.
- **Side effects**: Mutates the `LastLoginAt` property. The caller is responsible for persisting the change.

## Usage

### Example 1: Creating a new user and checking email validity

```csharp
var user = new User
{
    FirstName = "Jane",
    LastName = "Doe",
    Email = "jane.doe@example.com",
    PasswordHash = HashPassword("s3cret"),
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

if (!user.IsValidEmail)
{
    throw new InvalidOperationException("The email address is not in a valid format.");
}

Console.WriteLine($"Created user: {user.GetFullName}");
```

### Example 2: Recording a login and inspecting related data

```csharp
// Assume 'user' is an existing instance loaded from the database with Orders included.
user.UpdateLastLogin();

Console.WriteLine($"User {user.GetFullName} logged in at {user.LastLoginAt}.");

if (user.Orders?.Any() == true)
{
    var recentOrder = user.Orders.OrderByDescending(o => o.OrderDate).First();
    Console.WriteLine($"Most recent order placed on {recentOrder.OrderDate}.");
}
else
{
    Console.WriteLine("No orders found.");
}
```

## Notes

- **Thread safety**: This type is not designed to be thread-safe. Concurrent mutations to properties such as `LastLoginAt`, `UpdatedAt`, or navigation collections (`Orders`, `Reviews`) from multiple threads can lead to race conditions and data corruption. Synchronization must be handled externally when instances are shared across threads.
- **Nullability of navigation properties**: `Orders` and `Reviews` are nullable and may remain null if the related entities are not eagerly loaded or explicitly included in a query. Always perform a null check before iterating.
- **Email validation scope**: `IsValidEmail` performs a basic structural check only. It does not guarantee that the email is deliverable, that the domain accepts mail, or that the address is unique within the system. Use a confirmation workflow (e.g., `IsEmailVerified`) for authoritative ownership verification.
- **PasswordHash handling**: The `PasswordHash` property stores the hashed value. Assigning a plain-text password directly to this property bypasses any hashing logic and will prevent successful authentication. Always hash passwords before setting this field.
- **`UpdateLastLogin` precision**: The method sets `LastLoginAt` to `DateTime.UtcNow`. If called multiple times within the same persistence unit of work, only the last value will be stored. The method does not automatically persist the change; the caller must save the entity.
- **`GetFullName` edge cases**: If `FirstName` or `LastName` is null, the resulting string will contain the literal word "null" for the missing segment. Ensure both properties are assigned meaningful values before calling this method for display purposes.
