# UserRepository
The `UserRepository` class is designed to provide data access and retrieval functionality for user-related data in the application. It encapsulates the logic for interacting with the user data storage, providing methods for retrieving users based on various criteria, checking email existence, and obtaining user counts. This repository is built on top of the `AppDbContext` and inherits from a base class, indicating a structured approach to data access within the application.

## API
The `UserRepository` class exposes the following public members:
- `public UserRepository(AppDbContext context) : base(context)`: Constructs a new instance of the `UserRepository` class, taking an `AppDbContext` object as a parameter. This constructor is used to initialize the repository with the necessary database context.
- `public virtual async Task<User?> GetByEmailAsync`: Retrieves a user by their email address. The method returns a `User` object if found, or `null` if no user with the specified email exists. It may throw exceptions related to database access or query execution.
- `public virtual async Task<IEnumerable<User>> GetActiveUsersAsync`: Retrieves a collection of active users. The method returns an enumerable of `User` objects representing the active users. It may throw exceptions related to database access or query execution.
- `public virtual async Task<IEnumerable<User>> GetVerifiedUsersAsync`: Retrieves a collection of verified users. The method returns an enumerable of `User` objects representing the verified users. It may throw exceptions related to database access or query execution.
- `public virtual async Task<IEnumerable<User>> GetRecentlyActiveAsync`: Retrieves a collection of recently active users. The method returns an enumerable of `User` objects representing the recently active users. It may throw exceptions related to database access or query execution.
- `public virtual async Task<IEnumerable<User>> GetUsersByCountryAsync`: Retrieves a collection of users by their country. The method returns an enumerable of `User` objects representing the users from the specified country. It may throw exceptions related to database access or query execution.
- `public virtual async Task<int> GetUserCountAsync`: Retrieves the total count of users. The method returns an integer representing the total number of users. It may throw exceptions related to database access or query execution.
- `public virtual async Task<int> GetActiveUserCountAsync`: Retrieves the count of active users. The method returns an integer representing the number of active users. It may throw exceptions related to database access or query execution.
- `public virtual async Task<bool> EmailExistsAsync`: Checks if an email address exists in the user database. The method returns a boolean indicating whether the email address is found. It may throw exceptions related to database access or query execution.

## Usage
The following examples demonstrate how to use the `UserRepository` class:
```csharp
// Example 1: Retrieving a user by email
var context = new AppDbContext();
var userRepository = new UserRepository(context);
var user = await userRepository.GetByEmailAsync("example@example.com");
if (user != null)
{
    Console.WriteLine($"User found: {user.Name}");
}
else
{
    Console.WriteLine("User not found");
}

// Example 2: Retrieving active users and counting them
var activeUsers = await userRepository.GetActiveUsersAsync();
var activeUserCount = await userRepository.GetActiveUserCountAsync();
Console.WriteLine($"Active users: {activeUserCount}");
foreach (var user in activeUsers)
{
    Console.WriteLine($"User: {user.Name}");
}
```

## Notes
When using the `UserRepository` class, consider the following:
- The class is designed to work with the `AppDbContext`, which should be properly configured and initialized before creating an instance of the repository.
- The methods are asynchronous, allowing for non-blocking database operations. However, this also means that the calling code should be prepared to handle asynchronous execution and potential exceptions.
- The `GetByEmailAsync`, `GetActiveUsersAsync`, `GetVerifiedUsersAsync`, `GetRecentlyActiveAsync`, and `GetUsersByCountryAsync` methods may return large datasets, which can impact performance. It is essential to consider data pagination or filtering when retrieving large amounts of data.
- The `EmailExistsAsync` method is useful for validating user input, such as during registration or password recovery processes.
- The `UserRepository` class is not thread-safe by default. If used in a multi-threaded environment, proper synchronization mechanisms should be implemented to prevent concurrent access issues.
