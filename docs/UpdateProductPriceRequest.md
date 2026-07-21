# UpdateProductPriceRequest

Represents a request to update pricing information for a product, supporting batch price updates with detailed result tracking. The type encapsulates both the input parameters for price modifications and the aggregated outcome of the operation, including per-update results and summary counters.

## API

### PriceUpdates
**Type:** `required List<ProductPriceUpdate>`  
**Purpose:** Collection of individual price update specifications to apply to the product. Each entry defines a specific price change scenario such as currency, region, or customer tier adjustments.  
**When it throws:** `ArgumentNullException` if set to null; validation exceptions if any contained `ProductPriceUpdate` fails business rule validation.

### ProductId (required)
**Type:** `required int`  
**Purpose:** Identifier of the product whose prices are being updated. This is the primary key linking the request to the product entity.  
**When it throws:** `ArgumentException` if the value is less than or equal to zero.

### NewPrice (required)
**Type:** `required decimal`  
**Purpose:** The new base price value to apply. Serves as the default price when individual `PriceUpdates` do not specify an override. Must be non-negative.  
**When it throws:** `ArgumentOutOfRangeException` if the value is negative.

### TotalProcessed
**Type:** `int`  
**Purpose:** Total number of price update operations attempted during batch processing. Incremented for each item in `PriceUpdates` regardless of outcome.  
**Return value:** Read-only aggregate count populated after execution.

### SuccessCount
**Type:** `int`  
**Purpose:** Number of price updates that completed successfully. Subset of `TotalProcessed`.  
**Return value:** Read-only count populated after execution.

### FailureCount
**Type:** `int`  
**Purpose:** Number of price updates that failed during processing. Subset of `TotalProcessed`.  
**Return value:** Read-only count populated after execution.

### Results
**Type:** `required List<ProductPriceUpdateResult>`  
**Purpose:** Detailed per-update results corresponding to each entry in `PriceUpdates`. Contains success status, error details, and updated price identifiers for each operation.  
**When it throws:** `ArgumentNullException` if set to null.

### ProductId (response)
**Type:** `int`  
**Purpose:** Echoes the product identifier from the request in the response payload for correlation.  
**Return value:** The same value provided in the request's `ProductId`.

### ProductName
**Type:** `string?`  
**Purpose:** Human-readable name of the product, populated in the response for display purposes. Null if the product was not found or name resolution failed.  
**Return value:** Product name or null.

### Success
**Type:** `bool`  
**Purpose:** Indicates whether the overall batch operation succeeded. True only when `FailureCount` is zero.  
**Return value:** `true` if all updates succeeded; `false` otherwise.

### NewPrice (response)
**Type:** `decimal?`  
**Purpose:** The effective new price after processing, reflecting any overrides or adjustments applied. Null if the operation failed entirely.  
**Return value:** Final price value or null.

### ErrorMessage
**Type:** `string?`  
**Purpose:** Human-readable error description when the overall operation fails. Null on success.  
**Return value:** Error message or null.

### ErrorCode
**Type:** `string?`  
**Purpose:** Machine-readable error code for programmatic handling of failure scenarios. Null on success.  
**Return value:** Error code identifier or null.

## Usage

```csharp
var request = new UpdateProductPriceRequest
{
    ProductId = 12345,
    NewPrice = 29.99m,
    PriceUpdates = new List<ProductPriceUpdate>
    {
        new ProductPriceUpdate { Currency = "USD", Region = "NA", Price = 29.99m },
        new ProductPriceUpdate { Currency = "EUR", Region = "EU", Price = 27.50m },
        new ProductPriceUpdate { Currency = "GBP", Region = "UK", Price = 24.99m }
    }
};

var response = await _priceService.UpdateProductPricesAsync(request);

if (response.Success)
{
    _logger.LogInformation("Updated {Count} prices for product {ProductId}", 
        response.SuccessCount, response.ProductId);
}
else
{
    _logger.LogWarning("Price update failed for product {ProductId}: {ErrorCode} - {ErrorMessage}",
        response.ProductId, response.ErrorCode, response.ErrorMessage);
    
    foreach (var result in response.Results.Where(r => !r.Success))
    {
        _logger.LogDebug("Failed update for {Currency}/{Region}: {Error}", 
            result.Currency, result.Region, result.ErrorMessage);
    }
}
```

```csharp
public async Task<UpdateProductPriceRequest> BuildBulkPriceUpdateAsync(
    int productId, 
    decimal basePrice, 
    IEnumerable<RegionalPrice> regionalPrices,
    CancellationToken cancellationToken = default)
{
    var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
        ?? throw new ProductNotFoundException(productId);

    var request = new UpdateProductPriceRequest
    {
        ProductId = productId,
        NewPrice = basePrice,
        PriceUpdates = regionalPrices.Select(rp => new ProductPriceUpdate
        {
            Currency = rp.Currency,
            Region = rp.RegionCode,
            Price = rp.Amount,
            CustomerTier = rp.Tier
        }).ToList()
    };

    request.ProductName = product.Name;
    return request;
}
```

## Notes

- The type combines request input and response output fields in a single class, which is atypical for strict request/response separation. Consumers should treat `ProductId`, `NewPrice`, and `PriceUpdates` as input-only, and `TotalProcessed`, `SuccessCount`, `FailureCount`, `Results`, `ProductName`, `Success`, `NewPrice` (nullable), `ErrorMessage`, and `ErrorCode` as output-only populated by the service layer.
- Duplicate `ProductId` and `NewPrice` declarations (required vs optional) suggest a versioning artifact; the required variants are for request initialization, the optional variants for response population.
- Not thread-safe. Instances should not be shared across concurrent operations. Each price update batch should use a dedicated instance.
- `PriceUpdates` and `Results` maintain positional correspondence; the nth result corresponds to the nth update request. Do not reorder either collection after initialization.
- `TotalProcessed` equals `PriceUpdates.Count` after execution. `SuccessCount + FailureCount == TotalProcessed` invariant holds.
- When `Success` is false, `NewPrice` (nullable) may still contain a value if partial updates succeeded and the service returns the last successfully applied price.
- `ErrorCode` follows the project's standardized error code taxonomy (e.g., `PRICE_VALIDATION_FAILED`, `PRODUCT_NOT_FOUND`, `CURRENCY_NOT_SUPPORTED`).
