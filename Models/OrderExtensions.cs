#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq;

namespace AspNetSpaTemplate.Models
{
    /// <summary>
    /// Extension methods for <see cref="Order"/>.
    /// </summary>
    public static class OrderExtensions
    {
        /// <summary>
        /// Calculates the total amount of the order by summing each order item's
        /// subtotal, tax amount and subtracting any discount.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns>The total amount calculated from the items.</returns>
        public static decimal TotalAmount(this Order order)
        {
            if (order?.Items == null)
                return 0m;

            return order.Items.Sum(item =>
                (item.UnitPrice * item.Quantity) + // subtotal
                item.TaxAmount -                    // tax
                item.Discount);                     // discount
        }

        /// <summary>
        /// Returns the total quantity of all items in the order.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns>The sum of <c>Quantity</c> across all items.</returns>
        public static int ItemCount(this Order order)
        {
            if (order?.Items == null)
                return 0;

            return order.Items.Sum(item => item.Quantity);
        }

        /// <summary>
        /// Determines whether the order contains an item with the specified product identifier.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="productId">The product identifier to look for.</param>
        /// <returns><c>true</c> if any item has <c>ProductId</c> equal to <paramref name="productId"/>; otherwise, <c>false</c>.</returns>
        public static bool ContainsProduct(this Order order, int productId)
        {
            if (order?.Items == null)
                return false;

            return order.Items.Any(item => item.ProductId == productId);
        }
    }
}
