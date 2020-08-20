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

public sealed class UserServiceIntegrationTests : IAsyncLifetime
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private AppDbContext _dbContext = null!;
    private UserService _userService = null!;
    private UserRepository _userRepository = null!;

    public UserServiceIntegrationTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    public async Task InitializeAsync()
    {
        _dbContext = new AppDbContext(_dbOptions);
        await _dbContext.Database.EnsureCreatedAsync();
        _userRepository = new UserRepository(_dbContext);
        _userService = new UserService(_userRepository, NullLogger<UserService>.Instance);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }

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
