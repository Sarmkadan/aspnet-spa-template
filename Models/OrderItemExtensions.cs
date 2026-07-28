#nullable enable
using System;

namespace AspNetSpaTemplate.Models
{
    /// <summary>
    /// Extension methods for <see cref="OrderItem"/>.
    /// </summary>
    public static class OrderItemExtensions
    {
        /// <summary>
        /// Calculates the line total for an <see cref="OrderItem"/>.
        /// The calculation is: (UnitPrice × Quantity) – Discount.
        /// The result is rounded to two decimal places (currency precision).
        /// </summary>
        /// <param name="item">The order item.</param>
        /// <returns>The line total.</returns>
        public static decimal LineTotal(this OrderItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            var subtotal = item.UnitPrice * item.Quantity;
            var total = subtotal - item.Discount;

            // Guard against negative totals (should never happen, but be safe)
            if (total < 0) total = 0;

            return Math.Round(total, 2);
        }

        /// <summary>
        /// Indicates whether the order item has a discount applied.
        /// </summary>
        /// <param name="item">The order item.</param>
        /// <returns><c>true</c> if <see cref="OrderItem.Discount"/> is greater than zero; otherwise, <c>false</c>.</returns>
        public static bool IsDiscounted(this OrderItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            return item.Discount > 0m;
        }

        /// <summary>
        /// Returns a human‑readable string that shows quantity, unit price,
        /// discount (if any) and the calculated line total.
        /// Example without discount: 3 × $12.34 = $37.02
        /// Example with discount: 2 × $20.00 (−$2.00) = $38.00
        /// </summary>
        /// <param name="item">The order item.</param>
        /// <returns>A formatted display string.</returns>
        public static string ToDisplayString(this OrderItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            var lineTotal = item.LineTotal();

            if (item.IsDiscounted())
            {
                return $"{item.Quantity} × {item.UnitPrice:C} (−{item.Discount:C}) = {lineTotal:C}";
            }

            return $"{item.Quantity} × {item.UnitPrice:C} = {lineTotal:C}";
        }
    }
}
