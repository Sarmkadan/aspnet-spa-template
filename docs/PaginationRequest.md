# PaginationRequest
The `PaginationRequest` type is designed to facilitate paginated data retrieval, providing a structured approach to sorting, filtering, and searching data. It encapsulates various parameters and methods that enable efficient data pagination, making it easier to manage and display large datasets in a user-friendly manner.

## API
* `public string? SortBy`: Specifies the field to sort the data by. This property is optional and can be null.
* `public bool SortDescending`: Indicates whether the data should be sorted in descending order. Default is false.
* `public string? SearchTerm`: The term to search for in the data. This property is optional and can be null.
* `public Dictionary<string, string>? Filters`: A dictionary of filters to apply to the data, where the key is the field name and the value is the filter value. This property is optional and can be null.
* `public void Validate()`: Validates the pagination request. This method does not return any value but may throw an exception if the request is invalid.
* `public int GetSkip()`: Calculates the number of records to skip based on the page number and page size. Returns the number of records to skip.
* `public List<T> Items`: The list of items retrieved based on the pagination request.
* `public int PageNumber`: The current page number. Default is 1.
* `public int PageSize`: The number of items per page. Default is 10.
* `public int TotalCount`: The total number of items in the dataset.
* `public DateTime GeneratedAt`: The date and time the pagination request was generated.
* `public static PaginationResponse<T> Create()`: Creates a new instance of `PaginationResponse<T>`. Returns the created instance.
* `public string Field`: The field name for filtering or sorting.
* `public string Operator`: The operator to use for filtering (e.g., equals, contains, etc.).
* `public string Value`: The value to filter or sort by.
* `public bool IsValid`: Indicates whether the pagination request is valid.
* `public SortDirection Direction`: The direction of sorting (ascending or descending).
Note that some properties seem to be duplicated (e.g., `Field`), which might be due to the provided information. In a real-world scenario, these would likely be distinct properties serving different purposes or would be corrected to avoid redundancy.

## Usage
The following examples demonstrate how to use the `PaginationRequest` type:
```csharp
// Example 1: Basic pagination
var request = new PaginationRequest();
request.PageNumber = 2;
request.PageSize = 20;
request.SortBy = "Name";
request.SortDescending = true;

var response = PaginationResponse<MyType>.Create();
// Assuming MyType has a property named 'Name'

// Example 2: Filtering and searching
var filterRequest = new PaginationRequest();
filterRequest.Filters = new Dictionary<string, string>
{
    {"Category", "Electronics"},
    {"Price", "100"}
};
filterRequest.SearchTerm = "Laptop";
filterRequest.PageNumber = 1;
filterRequest.PageSize = 10;

var filteredResponse = PaginationResponse<MyType>.Create();
// Assuming MyType has properties named 'Category' and 'Price'
```

## Notes
When using `PaginationRequest`, consider the following:
- The `Validate` method should be called before attempting to retrieve data to ensure the request is valid. Failure to do so may result in unexpected behavior or exceptions.
- The `GetSkip` method relies on the `PageNumber` and `PageSize` properties being correctly set. Incorrect settings may lead to incorrect skipping of records.
- The `Filters` dictionary and `SearchTerm` property can significantly impact performance if not used judiciously, especially with large datasets. Ensure that database indexes are properly set up to support filtering and searching operations.
- This class appears to be designed for use in a web application or API, given the pagination and filtering capabilities. However, its thread-safety depends on the implementation details of the `PaginationResponse<T>` class and how instances of `PaginationRequest` are used and shared among threads. Generally, if `PaginationRequest` instances are not shared across threads or are properly synchronized, the class should be safe to use in multithreaded environments.
