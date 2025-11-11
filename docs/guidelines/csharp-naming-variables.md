# Use Meaningful Variable Names

Language: C#
Category: Naming
Tags: naming, readability, best-practices

## Description

Variables should have descriptive names that clearly indicate their purpose. Avoid single-letter names except for loop counters. Good variable names make code self-documenting and easier to understand.

## Guidelines

- Use descriptive names that reveal intent
- Avoid abbreviations unless they are widely known
- Use camelCase for local variables and parameters
- Use PascalCase for public properties and fields
- Prefix private fields with underscore `_`

## Examples

### Good Examples

```csharp
// Good - clear and descriptive
string customerName = "John Doe";
int orderCount = 42;
decimal totalPrice = 99.99m;
DateTime orderDate = DateTime.Now;

// Good - intention is clear
var customers = GetActiveCustomers();
var isValid = ValidateInput(userInput);
```

### Bad Examples

```csharp
// Bad - unclear abbreviations
string cn = "John Doe";
int oc = 42;
decimal tp = 99.99m;

// Bad - single letter variables (except for loops)
var c = GetActiveCustomers();
var v = ValidateInput(userInput);
```

## Loop Variables Exception

Single-letter variables are acceptable for loop counters:

```csharp
for (int i = 0; i < items.Length; i++)
{
    // i is acceptable here
}

foreach (var item in items)
{
    // item is descriptive enough
}
```

## References

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- [Clean Code by Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
