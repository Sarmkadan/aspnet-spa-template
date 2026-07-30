#nullable enable
using System.Collections.Generic;
using AspNetSpaTemplate.Models;
using Xunit;

namespace AspNetSpaTemplate.Tests.Models;

public class RatingSummaryExtensionsTests
{
    private RatingSummary CreateSampleSummary()
    {
        return new RatingSummary
        {
            AverageRating = 4.2m,
            ReviewCount = 10,
            StarCounts = new Dictionary<int, int>
            {
                { 5, 6 },
                { 4, 2 },
                { 3, 1 },
                { 2, 1 },
                { 1, 0 }
            }
        };
    }

    [Fact]
    public void StarPercentages_ReturnsCorrectPercentages()
    {
        var summary = CreateSampleSummary();

        var percentages = summary.StarPercentages();

        Assert.Equal(5, percentages.Count);
        Assert.Equal(60.0, percentages[5]); // 6/10 * 100
        Assert.Equal(20.0, percentages[4]); // 2/10 * 100
        Assert.Equal(10.0, percentages[3]); // 1/10 * 100
        Assert.Equal(10.0, percentages[2]); // 1/10 * 100
        Assert.Equal(0.0, percentages[1]);  // 0/10 * 100
    }

    [Fact]
    public void HasEnoughReviews_ReturnsTrueWhenEnough()
    {
        var summary = CreateSampleSummary();

        Assert.True(summary.HasEnoughReviews(5));
        Assert.True(summary.HasEnoughReviews(10));
        Assert.False(summary.HasEnoughReviews(11));
    }

    [Fact]
    public void FormattedAverage_ReturnsFormattedString()
    {
        var summary = CreateSampleSummary();

        Assert.Equal("4.2", summary.FormattedAverage());

        // When there are no reviews, the formatted value should be "N/A".
        summary.ReviewCount = 0;
        summary.AverageRating = 0;
        Assert.Equal("N/A", summary.FormattedAverage());
    }
}
