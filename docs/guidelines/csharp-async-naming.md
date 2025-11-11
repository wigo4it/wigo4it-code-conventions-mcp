# Async Methods Should End with Async

Language: C#
Category: Naming
Tags: naming, async, convention, async-await

## Description

All asynchronous methods should have an 'Async' suffix to clearly indicate their asynchronous nature. This convention helps developers quickly identify async methods and understand that they should be awaited.

## Guidelines

- Always append "Async" to method names that return `Task` or `Task<T>`
- Apply this rule even if the method is part of an interface
- Exception: Event handlers and interface implementations where the interface doesn't use Async suffix
- The suffix should be the last part of the method name

## Examples

### Good Examples

```csharp
public async Task<Customer> GetCustomerAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

public async Task SaveOrderAsync(Order order)
{
    await _repository.SaveAsync(order);
}

public async Task<List<Product>> SearchProductsAsync(string query)
{
    return await _searchService.SearchAsync(query);
}

public interface ICustomerService
{
    Task<Customer> GetCustomerAsync(int id);
    Task SaveCustomerAsync(Customer customer);
}
```

### Bad Examples

```csharp
// Bad - missing Async suffix
public async Task<Customer> GetCustomer(int id)
{
    return await _repository.GetByIdAsync(id);
}

// Bad - missing Async suffix
public async Task SaveOrder(Order order)
{
    await _repository.SaveAsync(order);
}

// Bad - inconsistent naming
public async Task<List<Product>> SearchProducts(string query)
{
    return await _searchService.SearchAsync(query);
}
```

## Exception Cases

### Event Handlers

Event handlers don't need the Async suffix:

```csharp
private async void OnButtonClicked(object sender, EventArgs e)
{
    await ProcessDataAsync();
}
```

### Interface Implementations

When implementing an interface that doesn't use Async suffix (e.g., older libraries), you may omit it:

```csharp
// Implementing a legacy interface
public async Task<string> Execute()
{
    return await GetResultAsync();
}
```

## Benefits

1. **Clarity**: Immediately identifies asynchronous operations
2. **Consistency**: Follows .NET framework conventions
3. **Searchability**: Easy to find all async methods in codebase
4. **Correctness**: Helps ensure methods are properly awaited

## References

- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
