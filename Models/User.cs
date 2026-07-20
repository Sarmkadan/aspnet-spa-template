#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
public sealed class User
{
    /// <summary>Gets or sets the user's unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the hashed password for the user.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's phone number (optional).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the user's street address (optional).</summary>
    public string? Address { get; set; }

    /// <summary>Gets or sets the user's city (optional).</summary>
    public string? City { get; set; }

    /// <summary>Gets or sets the user's postal code (optional).</summary>
    public string? PostalCode { get; set; }

    /// <summary>Gets or sets the user's country (optional).</summary>
    public string? Country { get; set; }

    /// <summary>Gets or sets whether the user account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether the user's email has been verified.</summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>Gets or sets when the user account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when the user account was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets when the user last logged in.</summary>
    public DateTime? LastLoginAt { get; set; }

/// <summary>Gets or sets when the user was deleted (soft delete).</summary>
public DateTime? DeletedAt { get; set; }

    /// <summary>Gets or sets the collection of orders placed by this user.</summary>
    public ICollection<Order>? Orders { get; set; }

    /// <summary>Gets or sets the collection of reviews written by this user.</summary>
    public ICollection<Review>? Reviews { get; set; }

    /// <summary>Gets the user's full name by combining first and last names.</summary>
    /// <returns>The concatenated full name.</returns>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    /// <summary>Validates if the email address is in a valid format.</summary>
    /// <returns>True if email contains '@' and is not empty; otherwise false.</returns>
    public bool IsValidEmail() => !string.IsNullOrWhiteSpace(Email) && Email.Contains("@");

    /// <summary>Updates the last login timestamp to current UTC time.</summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Updates the user's profile information.</summary>
    /// <param name="firstName">The new first name.</param>
    /// <param name="lastName">The new last name.</param>
    /// <param name="phone">The new phone number (optional).</param>
    /// <param name="address">The new address (optional).</param>
    /// <param name="city">The new city (optional).</param>
    /// <param name="postalCode">The new postal code (optional).</param>
    /// <param name="country">The new country (optional).</param>
    public void UpdateProfile(string firstName, string lastName, string? phone, string? address, string? city, string? postalCode, string? country)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phone;
        Address = address;
        City = city;
        PostalCode = postalCode;
        Country = country;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Marks the user's email as verified.</summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Deactivates the user account.</summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Activates the user account.</summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

/// <summary>Soft deletes the user account.</summary>
public void SoftDelete()
{
    DeletedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
}

/// <summary>Restores a soft-deleted user account.</summary>
public void Restore()
{
    DeletedAt = null;
    UpdatedAt = DateTime.UtcNow;
}

/// <summary>Checks if the user account is soft deleted.</summary>
/// <returns>True if the user is soft deleted; otherwise false.</returns>
public bool IsDeleted() => DeletedAt.HasValue;
}
