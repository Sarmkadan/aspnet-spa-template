# UserService

Provides operations for managing user entities, including creation, retrieval, updates, activation/deactivation, and authentication. Integrates with the application's data access layer and handles business logic for user state transitions.

## API

### `public UserService`

Constructor for the service. Initializes dependencies required for user operations, such as data access and authentication providers.

### `public async Task<UserResponse?> GetUserByIdAsync(int userId)`

Retrieves a user by their unique identifier.

- **Parameters**: `userId` – The unique identifier of the user to fetch.
- **Return value**: A `UserResponse` object if the user exists; otherwise, `null`.
- **Exceptions**: Throws if the underlying data access fails or if the user ID is invalid.

### `public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)`

Creates a new user in the system.

- **Parameters**: `request` – Contains user details such as username, email, and password.
- **Return value**: A `UserResponse` representing the newly created user.
- **Exceptions**: Throws if validation fails, the username/email is already in use, or the data access operation fails.

### `public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request)`

Updates an existing user's information.

- **Parameters**:
  - `userId` – The unique identifier of the user to update.
  - `request` – Contains updated user fields.
- **Return value**: A `UserResponse` reflecting the updated user.
- **Exceptions**: Throws if the user does not exist, validation fails, or the data access operation fails.

### `public async Task DeactivateUserAsync(int userId)`

Deactivates a user, preventing login and marking them as inactive.

- **Parameters**: `userId` – The unique identifier of the user to deactivate.
- **Exceptions**: Throws if the user does not exist or the deactivation fails.

### `public async Task ActivateUserAsync(int userId)`

Activates a previously deactivated user, enabling login and normal operations.

- **Parameters**: `userId` – The unique identifier of the user to activate.
- **Exceptions**: Throws if the user does not exist or the activation fails.

### `public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)`

Authenticates a user and returns a session token upon success.

- **Parameters**: `request` – Contains credentials such as username/email and password.
- **Return value**: A `LoginResponse` with authentication token and user details.
- **Exceptions**: Throws if credentials are invalid or the authentication process fails.

### `public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()`

Retrieves all active users in the system.

- **Return value**: An enumerable collection of `UserResponse` objects representing active users.
- **Exceptions**: Throws if the data access operation fails.

### `public async Task<IEnumerable<UserResponse>> GetRecentlyActiveUsersAsync(int days = 7)`

Retrieves users who have been active within the specified time window.

- **Parameters**: `days` – The number of days to look back for activity (default: 7).
- **Return value**: An enumerable collection of `UserResponse` objects for recently active users.
- **Exceptions**: Throws if the data access operation fails or the time window is invalid.

## Usage
