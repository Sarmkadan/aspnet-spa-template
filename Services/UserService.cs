#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for user-related business logic including registration, profile updates,
/// email uniqueness validation, and password hashing. Maps between domain entities
/// and DTOs for the API layer.
/// </summary>
public sealed class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Retrieves a user by their unique identifier and maps to a response DTO.
    /// </summary>
    /// <param name="id">The user's database identifier.</param>
    /// <returns>A <see cref="UserResponse"/> DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when no user exists with the given ID.</exception>
    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        return MapToResponse(user);
    }

    /// <summary>
    /// Creates a new user after validating the request and checking email uniqueness.
    /// The password is hashed before storage.
    /// </summary>
    /// <param name="request">The user creation request with profile data and credentials.</param>
    /// <returns>A <see cref="UserResponse"/> DTO for the newly created user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ValidationException">Thrown when the email already exists or request data is invalid.</exception>
    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        ValidateCreateUserRequest(request);

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ValidationException("Email", "Email already exists");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower(),
            PasswordHash = HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _userRepository.Add(user);
            await _userRepository.SaveChangesAsync();

            return MapToResponse(user);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new BusinessException("Failed to create user due to database error", "USER_CREATION_FAILED", 500).WithData(ex);
        }
    }

    /// <summary>
    /// Updates an existing user's profile fields. Does not modify email or password.
    /// </summary>
    /// <param name="id">The user's database identifier.</param>
    /// <param name="request">The profile update request with new field values.</param>
    /// <returns>The updated <see cref="UserResponse"/> DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when no user exists with the given ID.</exception>
    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.PostalCode,
            request.Country
        );

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task DeactivateUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.Deactivate();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ActivateUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.Activate();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
            throw new BusinessException("Invalid email or password", "INVALID_CREDENTIALS");

        if (!user.IsActive)
            throw new BusinessException("User account is inactive", "ACCOUNT_INACTIVE");

        user.UpdateLastLogin();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.GetFullName(),
            Token = GenerateToken(user)
        };
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        return users.Select(MapToResponse).ToList();
    }

    public async Task<IEnumerable<UserResponse>> GetRecentlyActiveUsersAsync(int days = 30)
    {
        var users = await _userRepository.GetRecentlyActiveAsync(days);
        return users.Select(MapToResponse).ToList();
    }

    private void ValidateCreateUserRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Length < 2)
            throw new ValidationException("FirstName", "First name must be at least 2 characters");

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Length < 2)
            throw new ValidationException("LastName", "Last name must be at least 2 characters");

        if (!request.Email.Contains("@") || request.Email.Length < 5)
            throw new ValidationException("Email", "Email is invalid");

        if (request.Password.Length < 8)
            throw new ValidationException("Password", "Password must be at least 8 characters");
    }

    private string HashPassword(string password)
    {
        // In production, use proper password hashing like bcrypt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateToken(User user)
    {
        // In production, implement proper JWT token generation
        return $"token_{user.Id}_{Guid.NewGuid()}";
    }

    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            City = user.City,
            PostalCode = user.PostalCode,
            Country = user.Country,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
