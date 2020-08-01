#nullable enable
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class UserServiceTests
{
    private readonly Mock<UserRepository> _mockUserRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<UserRepository>();
        _userService = new UserService(_mockUserRepository.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@example.com", IsActive = true };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result?.FirstName.Should().Be("John");
        result?.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var act = () => _userService.GetUserByIdAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateUserAsync_WithValidRequest_CreatesUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Password = "SecurePassword123!",
            PhoneNumber = "1234567890",
            Address = "123 Main St",
            City = "Boston",
            PostalCode = "02101",
            Country = "USA"
        };
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.Add(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("jane@example.com");
        _mockUserRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "existing@example.com",
            Password = "SecurePassword123!",
            PhoneNumber = null,
            Address = null,
            City = null,
            PostalCode = null,
            Country = null
        };
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(true);

        // Act
        var act = () => _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUserAsync_WithShortFirstName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "A",
            LastName = "Smith",
            Email = "jane@example.com",
            Password = "SecurePassword123!",
            PhoneNumber = null,
            Address = null,
            City = null,
            PostalCode = null,
            Country = null
        };

        // Act
        var act = () => _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid",
            Password = "SecurePassword123!",
            PhoneNumber = null,
            Address = null,
            City = null,
            PostalCode = null,
            Country = null
        };

        // Act
        var act = () => _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUserAsync_WithShortPassword_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "short",
            PhoneNumber = null,
            Address = null,
            City = null,
            PostalCode = null,
            Country = null
        };

        // Act
        var act = () => _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidRequest_UpdatesProfile()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, FirstName = "John", LastName = "Doe" };
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "9999999999",
            Address = "456 Oak Ave",
            City = "NYC",
            PostalCode = "10001",
            Country = "USA"
        };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.UpdateUserAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Id = 1, Email = "test@example.com", PasswordHash = hashedPassword, IsActive = true };
        var request = new LoginRequest { Email = "test@example.com", Password = password };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.AuthenticateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.Token.Should().NotBeEmpty();
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ThrowsBusinessException()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Id = 1, Email = "test@example.com", PasswordHash = hashedPassword, IsActive = true };
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        // Act
        var act = () => _userService.AuthenticateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ThrowsBusinessException()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@example.com", Password = "Password123!" };
        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        var act = () => _userService.AuthenticateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUser_ThrowsBusinessException()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Id = 1, Email = "test@example.com", PasswordHash = hashedPassword, IsActive = false };
        var request = new LoginRequest { Email = "test@example.com", Password = password };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        // Act
        var act = () => _userService.AuthenticateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*inactive*");
    }

    [Fact]
    public async Task DeactivateUserAsync_WithValidId_DeactivatesUser()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, IsActive = true };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _userService.DeactivateUserAsync(userId);

        // Assert
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_WithValidId_ActivatesUser()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, IsActive = false };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _userService.ActivateUserAsync(userId);

        // Assert
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, FirstName = "John", IsActive = true },
            new User { Id = 2, FirstName = "Jane", IsActive = true }
        };
        _mockUserRepository.Setup(r => r.GetActiveUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
