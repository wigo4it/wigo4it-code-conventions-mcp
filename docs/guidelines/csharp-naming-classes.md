# PascalCase for Class Names

Language: C#
Category: Naming
Tags: naming, convention, classes

## Description

All class names should use PascalCase convention where each word starts with a capital letter. This is a widely adopted convention in the .NET ecosystem and helps maintain consistency across codebases.

## Guidelines

- Start each word with a capital letter
- No underscores or hyphens
- Use nouns or noun phrases
- Be specific and descriptive
- Avoid generic names like `Manager`, `Helper`, `Utility` unless necessary

## Examples

### Good Examples

```csharp
public class CustomerService { }
public class OrderRepository { }
public class EmailNotificationSender { }
public class PaymentProcessor { }
public class UserAuthenticationValidator { }
```

### Bad Examples

```csharp
// Bad - camelCase
public class customerService { }

// Bad - lowercase
public class orderrepository { }

// Bad - snake_case
public class email_notification_sender { }

// Bad - all caps
public class PAYMENTPROCESSOR { }

// Bad - too generic
public class Manager { }
public class Helper { }
```

## Interface Naming

Interfaces should follow the same PascalCase convention but with an 'I' prefix:

```csharp
public interface ICustomerService { }
public interface IOrderRepository { }
public interface IEmailSender { }
```

## Abstract Classes

Abstract classes follow the same PascalCase convention without special prefixes:

```csharp
public abstract class BaseRepository { }
public abstract class EntityBase { }
```

## References

- [C# Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)
- [.NET Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
