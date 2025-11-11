# C# Coding Style Guide

Language: C#
Tags: csharp, formatting, style, conventions

## Overview

This document outlines the coding style conventions for C# development at Wigo4it. Following these conventions ensures consistency across our codebase and improves readability and maintainability.

## Naming Conventions

### Classes and Structs
- Use **PascalCase**
- Use nouns or noun phrases
- Example: `CustomerService`, `OrderProcessor`

### Interfaces
- Use **PascalCase** with 'I' prefix
- Example: `ICustomerRepository`, `IEmailSender`

### Methods
- Use **PascalCase**
- Use verbs or verb phrases
- Async methods should end with "Async"
- Example: `GetCustomer()`, `ProcessOrderAsync()`

### Properties
- Use **PascalCase**
- Example: `CustomerName`, `OrderTotal`

### Private Fields
- Use **camelCase** with underscore prefix
- Example: `_customerRepository`, `_logger`

### Local Variables and Parameters
- Use **camelCase**
- Example: `customerName`, `orderId`

### Constants
- Use **PascalCase**
- Example: `MaxRetryCount`, `DefaultTimeout`

## Formatting

### Indentation
- Use **4 spaces** for indentation (not tabs)
- Configure your editor to insert spaces when pressing Tab

### Braces
- Opening braces on **new line** (Allman style)
- Always use braces for control structures, even single-line statements

```csharp
// Good
if (condition)
{
    DoSomething();
}

// Bad
if (condition) DoSomething();
```

### Line Length
- Keep lines under **120 characters** when possible
- Break long lines at logical points

### Blank Lines
- One blank line between methods
- One blank line between property groups
- Two blank lines between class declarations in the same file (avoid if possible)

### Spacing
- One space after keywords: `if (`, `for (`, `while (`
- One space around operators: `x + y`, `a == b`
- No space after opening parenthesis or before closing: `Method(param)`

## Code Organization

### File Organization
```csharp
// 1. Using statements
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

// 2. Namespace
namespace Wigo4it.MyProject.Services;

// 3. Class declaration
public class CustomerService
{
    // 4. Fields
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;

    // 5. Constructor
    public CustomerService(
        ICustomerRepository repository,
        ILogger<CustomerService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 6. Properties
    public int MaxRetries { get; set; } = 3;

    // 7. Public methods
    public async Task<Customer> GetCustomerAsync(int id)
    {
        // Implementation
    }

    // 8. Private methods
    private void LogError(string message)
    {
        // Implementation
    }
}
```

### Using Statements
- Group and order using statements:
  1. System namespaces
  2. Third-party namespaces
  3. Application namespaces
- Use file-scoped namespaces (C# 10+)

```csharp
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Wigo4it.Core.Services;

namespace Wigo4it.MyProject;
```

## Best Practices

### var Keyword
- Use `var` when type is obvious from the right side
- Use explicit type when it improves readability

```csharp
// Good - type is obvious
var customer = new Customer();
var customers = GetCustomers();

// Good - explicit type for clarity
string connectionString = Configuration.GetConnectionString();
int maxRetries = 3;
```

### String Interpolation
- Prefer string interpolation over `string.Format` or concatenation

```csharp
// Good
string message = $"Customer {name} has {orderCount} orders";

// Bad
string message = "Customer " + name + " has " + orderCount + " orders";
```

### Null Checking
- Use null-coalescing and null-conditional operators

```csharp
// Good
string name = customer?.Name ?? "Unknown";
int count = orders?.Count ?? 0;

// Modern C# (10+)
ArgumentNullException.ThrowIfNull(customer);
```

### LINQ
- Use LINQ for collection operations
- Use method syntax for simple queries
- Use query syntax for complex queries with multiple clauses

```csharp
// Method syntax
var activeCustomers = customers.Where(c => c.IsActive).ToList();

// Query syntax for complex queries
var result = from customer in customers
             join order in orders on customer.Id equals order.CustomerId
             where order.Total > 100
             select new { customer.Name, order.Total };
```

### Async/Await
- Always await async methods (avoid `.Result` or `.Wait()`)
- Use `ConfigureAwait(false)` in library code
- Don't use `async void` except for event handlers

```csharp
// Good
public async Task<Customer> GetCustomerAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// Bad
public Customer GetCustomer(int id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}
```

### IDisposable
- Always use `using` statements or declarations for `IDisposable` objects

```csharp
// Modern using declaration
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

// Traditional using statement
using (var stream = File.OpenRead(path))
{
    // Use stream
}
```

## Comments and Documentation

### XML Documentation
- Use XML comments for public APIs
- Include summary, parameter descriptions, and return values

```csharp
/// <summary>
/// Retrieves a customer by their unique identifier.
/// </summary>
/// <param name="id">The unique customer identifier.</param>
/// <returns>The customer if found; otherwise, null.</returns>
public async Task<Customer?> GetCustomerAsync(int id)
{
    // Implementation
}
```

### Inline Comments
- Use comments to explain "why", not "what"
- Keep comments up-to-date with code changes
- Avoid obvious comments

```csharp
// Good - explains why
// Retry logic needed due to intermittent network issues
for (int i = 0; i < maxRetries; i++)
{
    // Implementation
}

// Bad - states the obvious
// Loop through the array
for (int i = 0; i < items.Length; i++)
{
    // Implementation
}
```

## References

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Clean Code by Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
