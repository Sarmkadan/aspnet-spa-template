#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Provides validation helpers for <see cref="OrderServiceIntegrationTests"/> instances.
/// </summary>
public static class OrderServiceIntegrationTestsValidation
{
    private static readonly FieldInfo? _dbOptionsField = typeof(OrderServiceIntegrationTests).GetField("_dbOptions", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _dbContextField = typeof(OrderServiceIntegrationTests).GetField("_dbContext", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _orderServiceField = typeof(OrderServiceIntegrationTests).GetField("_orderService", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _productServiceField = typeof(OrderServiceIntegrationTests).GetField("_productService", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _orderRepositoryField = typeof(OrderServiceIntegrationTests).GetField("_orderRepository", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _productRepositoryField = typeof(OrderServiceIntegrationTests).GetField("_productRepository", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? _initializeAsyncMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.InitializeAsync));
    private static readonly MethodInfo? _disposeAsyncMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.DisposeAsync));
    private static readonly MethodInfo? _endToEndMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.EndToEnd_CreateProductAndOrder_CompleteWorkflow));
    private static readonly MethodInfo? _multipleItemsMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.CreateOrderWithMultipleItems_CalculatesTotalsCorrectly));
    private static readonly MethodInfo? _stockReductionMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.StockReduction_DecreasesProductInventory));
    private static readonly MethodInfo? _insufficientStockMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.InsufficientStock_PreventOrderCreation));
    private static readonly MethodInfo? _getUserOrdersMethod = typeof(OrderServiceIntegrationTests).GetMethod(nameof(OrderServiceIntegrationTests.GetUserOrders_ReturnsOnlyUserOrders));

    /// <summary>
    /// Validates the specified <see cref="OrderServiceIntegrationTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this OrderServiceIntegrationTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate field dependencies using reflection
        ValidateField(value, _dbOptionsField, "_dbOptions", problems);
        ValidateField(value, _dbContextField, "_dbContext", problems);
        ValidateField(value, _orderServiceField, "_orderService", problems);
        ValidateField(value, _productServiceField, "_productService", problems);
        ValidateField(value, _orderRepositoryField, "_orderRepository", problems);
        ValidateField(value, _productRepositoryField, "_productRepository", problems);

        // Validate public methods
        ValidateMethod(_initializeAsyncMethod, nameof(OrderServiceIntegrationTests.InitializeAsync), problems);
        ValidateMethod(_disposeAsyncMethod, nameof(OrderServiceIntegrationTests.DisposeAsync), problems);
        ValidateMethod(_endToEndMethod, nameof(OrderServiceIntegrationTests.EndToEnd_CreateProductAndOrder_CompleteWorkflow), problems);
        ValidateMethod(_multipleItemsMethod, nameof(OrderServiceIntegrationTests.CreateOrderWithMultipleItems_CalculatesTotalsCorrectly), problems);
        ValidateMethod(_stockReductionMethod, nameof(OrderServiceIntegrationTests.StockReduction_DecreasesProductInventory), problems);
        ValidateMethod(_insufficientStockMethod, nameof(OrderServiceIntegrationTests.InsufficientStock_PreventOrderCreation), problems);
        ValidateMethod(_getUserOrdersMethod, nameof(OrderServiceIntegrationTests.GetUserOrders_ReturnsOnlyUserOrders), problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="OrderServiceIntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this OrderServiceIntegrationTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="OrderServiceIntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the validation problems.</exception>
    public static void EnsureValid(this OrderServiceIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"OrderServiceIntegrationTests instance is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    private static void ValidateField(OrderServiceIntegrationTests instance, FieldInfo? field, string fieldName, List<string> problems)
    {
        if (field is null)
        {
            problems.Add($"Field '{fieldName}' is not found on OrderServiceIntegrationTests.");
            return;
        }

        var fieldValue = field.GetValue(instance);
        if (fieldValue is null)
        {
            problems.Add($"Field '{fieldName}' is null.");
        }
    }

    private static void ValidateMethod(MethodInfo? method, string methodName, List<string> problems)
    {
        if (method is null)
        {
            problems.Add($"Method '{methodName}' is not found.");
        }
        else if (method.DeclaringType != typeof(OrderServiceIntegrationTests))
        {
            problems.Add($"Method '{methodName}' is not declared on OrderServiceIntegrationTests.");
        }
    }
}