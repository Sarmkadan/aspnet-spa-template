#nullable enable
using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Integration tests for <see cref="UserService"/> that verify end-to-end user management workflows
/// including registration, authentication, profile updates, and account lifecycle operations.
/// Uses in-memory database for isolated testing without external dependencies.
/// </summary>
public sealed class UserServiceIntegrationTests : IAsyncLifetime
{
	private readonly DbContextOptions<AppDbContext> _dbOptions;
	private AppDbContext _dbContext = null!;
	private UserService _userService = null!;
	private UserRepository _userRepository = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="UserServiceIntegrationTests"/> class.
	/// Configures an in-memory database with a unique name for each test run to ensure isolation.
	/// </summary>
	public UserServiceIntegrationTests()
	{
		_dbOptions = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
			.Options;
	}

	/// <summary>
	/// Initializes the test database and creates the required services for testing.
	/// Sets up an in-memory database, ensures schema is created, and initializes
	/// the UserRepository and UserService with null logger.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task InitializeAsync()
	{
		_dbContext = new AppDbContext(_dbOptions);
		await _dbContext.Database.EnsureCreatedAsync();
		_userRepository = new UserRepository(_dbContext);
		_userService = new UserService(_userRepository, NullLogger<UserService>.Instance);
	}

	/// <summary>
	/// Cleans up the test database after each test completes.
	/// Ensures the in-memory database is deleted and disposes the database context.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DisposeAsync()
	{
		await _dbContext.Database.EnsureDeletedAsync();
		_dbContext.Dispose();
	}

	/// <summary>
	/// Tests a complete user authentication flow from registration to profile update.
	/// Verifies that a user can register, login, and update their profile information successfully.
	/// </summary>
	[Fact]
	public async Task EndToEnd_RegisterUserLoginAndUpdate_CompleteAuthFlow()
	{
		// Arrange
		var registerRequest = new CreateUserRequest
		{
			FirstName = "John",
			LastName = "Doe",
			Email = "john@example.com",
			Password = "SecurePass123!",
			PhoneNumber = "1234567890",
			Address = "123 Main St",
			City = "Boston",
			PostalCode = "02101",
			Country = "USA"
		};

		// Act - Register User
		var userResponse = await _userService.CreateUserAsync(registerRequest);

		// Assert - User Created
		userResponse.Should().NotBeNull();
		userResponse.FirstName.Should().Be("John");
		userResponse.Email.Should().Be("john@example.com");
		userResponse.IsActive.Should().BeTrue();

		// Act - Login
		var loginRequest = new LoginRequest { Email = "john@example.com", Password = "SecurePass123!" };
		var loginResponse = await _userService.AuthenticateAsync(loginRequest);

		// Assert - Login Successful
		loginResponse.Should().NotBeNull();
		loginResponse.Token.Should().NotBeEmpty();
		loginResponse.UserId.Should().BeGreaterThan(0);

		// Act - Update Profile
		var updateRequest = new UpdateUserRequest
		{
			FirstName = "Jonathan",
			LastName = "Smith",
			PhoneNumber = "9876543210",
			Address = "456 Oak Ave",
			City = "NYC",
			PostalCode = "10001",
			Country = "USA"
		};
		var updatedUser = await _userService.UpdateUserAsync(userResponse.Id, updateRequest);

		// Assert - Profile Updated
		updatedUser.Should().NotBeNull();
		updatedUser.FirstName.Should().Be("Jonathan");
		updatedUser.LastName.Should().Be("Smith");
	}

	/// <summary>
	/// Tests that multiple users with unique emails can all be created successfully.
	/// Verifies that the user service properly handles concurrent user creation
	/// and maintains data integrity across multiple user records.
	/// </summary>
	[Fact]
	public async Task MultipleUsersWithUniqueEmails_CanAllBeCreated()
	{
		// Arrange
		var users = new[]
		{
			new CreateUserRequest { FirstName = "User1", LastName = "One", Email = "user1@test.com", Password = "Password123!", PhoneNumber = null, Address = null, City = null, PostalCode = null, Country = null },
			new CreateUserRequest { FirstName = "User2", LastName = "Two", Email = "user2@test.com", Password = "Password123!", PhoneNumber = null, Address = null, City = null, PostalCode = null, Country = null },
			new CreateUserRequest { FirstName = "User3", LastName = "Three", Email = "user3@test.com", Password = "Password123!", PhoneNumber = null, Address = null, City = null, PostalCode = null, Country = null }
		};

		// Act & Assert
		foreach (var userRequest in users)
		{
			var response = await _userService.CreateUserAsync(userRequest);
			response.Should().NotBeNull();
			response.Email.Should().Be(userRequest.Email.ToLower());
		}

		var allUsers = await _userService.GetAllUsersAsync();
		allUsers.Should().HaveCount(3);
	}

	/// <summary>
	/// Tests that attempting to create a second user with a duplicate email throws a validation exception.
	/// Verifies that the user service enforces unique email constraints.
	/// </summary>
	[Fact]
	public async Task DuplicateEmail_PreventSecondUserCreation()
	{
		// Arrange
		var request = new CreateUserRequest
		{
			FirstName = "John",
			LastName = "Doe",
			Email = "duplicate@example.com",
			Password = "SecurePass123!",
			PhoneNumber = null,
			Address = null,
			City = null,
			PostalCode = null,
			Country = null
		};

		// Act - Create first user
		await _userService.CreateUserAsync(request);

		// Act & Assert - Try to create second with same email
		await Assert.ThrowsAsync<ValidationException>(() => _userService.CreateUserAsync(request));
	}

	/// <summary>
	/// Tests user account deactivation and reactivation functionality.
	/// Verifies that deactivated users cannot login and reactivated users can login again.
	/// </summary>
	[Fact]
	public async Task DeactivateAndReactivate_UserAccessibility()
	{
		// Arrange
		var registerRequest = new CreateUserRequest
		{
			FirstName = "Test",
			LastName = "User",
			Email = "test@example.com",
			Password = "SecurePass123!",
			PhoneNumber = null,
			Address = null,
			City = null,
			PostalCode = null,
			Country = null
		};

		var user = await _userService.CreateUserAsync(registerRequest);

		// Act - Deactivate
		await _userService.DeactivateUserAsync(user.Id);

		// Assert - Cannot login with inactive account
		var loginRequest = new LoginRequest { Email = "test@example.com", Password = "SecurePass123!" };
		await Assert.ThrowsAsync<BusinessException>(() => _userService.AuthenticateAsync(loginRequest));

		// Act - Reactivate
		await _userService.ActivateUserAsync(user.Id);

		// Assert - Can login again
		var loginResponse = await _userService.AuthenticateAsync(loginRequest);
		loginResponse.Should().NotBeNull();
	}

	/// <summary>
	/// Tests that the last login timestamp is updated when a user authenticates.
	/// Verifies that the authentication process properly tracks user activity.
	/// </summary>
	[Fact]
	public async Task LastLoginTimestamp_UpdatedOnAuthentication()
	{
		// Arrange
		var registerRequest = new CreateUserRequest
		{
			FirstName = "Track",
			LastName = "Login",
			Email = "track@example.com",
			Password = "SecurePass123!",
			PhoneNumber = null,
			Address = null,
			City = null,
			PostalCode = null,
			Country = null
		};

		var user = await _userService.CreateUserAsync(registerRequest);
		var userBefore = await _userService.GetUserByIdAsync(user.Id);
		var initialLastLogin = userBefore?.LastLoginAt;

		await Task.Delay(100);

		// Act
		var loginRequest = new LoginRequest { Email = "track@example.com", Password = "SecurePass123!" };
		await _userService.AuthenticateAsync(loginRequest);

		// Assert
		var userAfter = await _userService.GetUserByIdAsync(user.Id);
		userAfter?.LastLoginAt.Should().NotBe(initialLastLogin);
		userAfter?.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
	}

	/// <summary>
	/// Tests that password validation enforces minimum length requirements.
	/// Verifies that weak passwords are rejected by the user service.
	/// </summary>
	[Fact]
	public async Task PasswordValidation_EnforcesMinimumLength()
	{
		// Arrange
		var weakPasswordRequest = new CreateUserRequest
		{
			FirstName = "Weak",
			LastName = "Pass",
			Email = "weak@example.com",
			Password = "short",
			PhoneNumber = null,
			Address = null,
			City = null,
			PostalCode = null,
			Country = null
		};

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() => _userService.CreateUserAsync(weakPasswordRequest));
	}

	/// <summary>
	/// Tests that GetAllUsersAsync only returns active users, excluding inactive ones.
	/// Verifies that the user service properly filters users by active status.
	/// </summary>
	[Fact]
	public async Task GetActiveUsers_ExcludesInactiveUsers()
	{
		// Arrange
		var requests = new[]
		{
			new CreateUserRequest { FirstName = "Active1", LastName = "User", Email = "active1@test.com", Password = "Pass123!", PhoneNumber = null, Address = null, City = null, PostalCode = null, Country = null },
			new CreateUserRequest { FirstName = "Active2", LastName = "User", Email = "active2@test.com", Password = "Pass123!", PhoneNumber = null, Address = null, City = null, PostalCode = null, Country = null },
			new CreateUserRequest { FirstName = "Inactive", LastName = "User", Email = "inactive@test.com", Password = "Pass123!", PhoneNumber = null, Address = null, City = null, PostalCode = null, Country = null }
		};

		var users = new List<int>();
		foreach (var request in requests)
		{
			var user = await _userService.CreateUserAsync(request);
			users.Add(user.Id);
		}

		await _userService.DeactivateUserAsync(users[2]);

		// Act
		var activeUsers = await _userService.GetAllUsersAsync();

		// Assert
		activeUsers.Should().HaveCount(2);
		activeUsers.Should().AllSatisfy(u => u.IsActive.Should().BeTrue());
	}
}