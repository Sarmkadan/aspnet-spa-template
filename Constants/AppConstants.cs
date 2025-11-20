// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Constants;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class AppConstants
{
    public const string ApiVersion = "v1";
    public const string ApiBaseRoute = "/api/" + ApiVersion;

    public static class Validation
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 255;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MaxDescriptionLength = 5000;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;
    }

    public static class Product
    {
        public const decimal MinPrice = 0.01m;
        public const decimal MaxPrice = 999999.99m;
        public const int MinStock = 0;
        public const int MaxStock = 1000000;
    }

    public static class Order
    {
        public const int MinItemQuantity = 1;
        public const int MaxItemQuantity = 10000;
        public const decimal MinOrderValue = 0.01m;
    }

    public static class Cache
    {
        public const int DefaultDurationMinutes = 30;
        public const int ShortDurationMinutes = 5;
        public const int LongDurationMinutes = 120;
    }
}
