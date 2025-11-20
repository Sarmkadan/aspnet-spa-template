// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a product review by a user.
/// </summary>
public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int HelpfulCount { get; set; } = 0;
    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsApproved { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Product? Product { get; set; }
    public User? User { get; set; }

    public bool IsValidRating() => Rating >= 1 && Rating <= 5;

    public bool IsRecent(int days = 30) => DateTime.UtcNow.Subtract(CreatedAt).TotalDays <= days;

    public void MarkAsHelpful()
    {
        HelpfulCount++;
    }

    public void UpdateReview(int rating, string title, string content)
    {
        if (!IsValidRating())
            throw new ArgumentException("Rating must be between 1 and 5");

        Rating = rating;
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetRatingDisplay() => Rating switch
    {
        1 => "★☆☆☆☆",
        2 => "★★☆☆☆",
        3 => "★★★☆☆",
        4 => "★★★★☆",
        5 => "★★★★★",
        _ => "No rating"
    };

    public bool CanBeEdited() => DateTime.UtcNow.Subtract(CreatedAt).TotalDays < 30;
}
