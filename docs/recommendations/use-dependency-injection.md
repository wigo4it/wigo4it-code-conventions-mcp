# Use Dependency Injection

Category: Architecture
Tags: dependency-injection, solid, testability, architecture

## Overview

Dependency Injection (DI) is a fundamental design pattern that should be used throughout the application to manage dependencies between components. This recommendation applies to all new code and refactoring efforts.

## Rationale

1. **Testability**: DI makes unit testing easier by allowing dependencies to be mocked or stubbed
2. **Loose Coupling**: Components depend on abstractions (interfaces) rather than concrete implementations
3. **Flexibility**: Easy to swap implementations without changing dependent code
4. **Maintainability**: Clear dependency graph makes code easier to understand and maintain
5. **Framework Support**: Built-in support in .NET through `Microsoft.Extensions.DependencyInjection`

## Guidelines

### Register Services in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddTransient<IOrderProcessor, OrderProcessor>();

var app = builder.Build();
```

### Use Constructor Injection

```csharp
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;
    private readonly IEmailSender _emailSender;

    public CustomerService(
        ICustomerRepository repository,
        ILogger<CustomerService> logger,
        IEmailSender emailSender)
    {
        _repository = repository;
        _logger = logger;
        _emailSender = emailSender;
    }

    public async Task<Customer> GetCustomerAsync(int id)
    {
        _logger.LogInformation("Getting customer {CustomerId}", id);
        return await _repository.GetByIdAsync(id);
    }
}
```

### Service Lifetimes

Choose appropriate lifetime for each service:

- **Transient**: Created each time they're requested
  - Lightweight, stateless services
  - Example: `IOrderProcessor`, `IValidator`

- **Scoped**: Created once per request/scope
  - Services with request-specific state
  - Example: `ICustomerRepository`, `DbContext`

- **Singleton**: Created once for the application lifetime
  - Stateless services or services with application-wide state
  - Example: `IConfiguration`, `IMemoryCache`

```csharp
// Transient - new instance every time
builder.Services.AddTransient<IOrderValidator, OrderValidator>();

// Scoped - one instance per request
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Singleton - one instance for application
builder.Services.AddSingleton<IEmailSender, EmailSender>();
```

## Anti-Patterns to Avoid

### Service Locator Pattern

```csharp
// Bad - Service Locator anti-pattern
public class CustomerService
{
    public void ProcessOrder(Order order)
    {
        var repository = ServiceLocator.Get<IOrderRepository>();
        repository.Save(order);
    }
}
```

### Static Dependencies

```csharp
// Bad - static dependency
public class CustomerService
{
    public void SendEmail(Customer customer)
    {
        EmailSender.Send(customer.Email, "Welcome!");
    }
}
```

### new Keyword for Services

```csharp
// Bad - creating dependencies with new
public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService()
    {
        _repository = new CustomerRepository(); // Don't do this!
    }
}
```

## Testing with DI

DI makes testing straightforward:

```csharp
[Fact]
public async Task GetCustomer_ReturnsCustomer_WhenExists()
{
    // Arrange
    var mockRepository = new Mock<ICustomerRepository>();
    var mockLogger = new Mock<ILogger<CustomerService>>();
    var mockEmailSender = new Mock<IEmailSender>();
    
    mockRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(new Customer { Id = 1, Name = "John" });

    var service = new CustomerService(
        mockRepository.Object,
        mockLogger.Object,
        mockEmailSender.Object);

    // Act
    var result = await service.GetCustomerAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("John", result.Name);
}
```

## Advanced Scenarios

### Factory Pattern with DI

```csharp
public interface IProcessorFactory
{
    IOrderProcessor Create(string orderType);
}

public class ProcessorFactory : IProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOrderProcessor Create(string orderType)
    {
        return orderType switch
        {
            "Standard" => _serviceProvider.GetRequiredService<StandardOrderProcessor>(),
            "Express" => _serviceProvider.GetRequiredService<ExpressOrderProcessor>(),
            _ => throw new ArgumentException($"Unknown order type: {orderType}")
        };
    }
}
```

### Conditional Registration

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailSender, FakeEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}
```

### Multiple Implementations

```csharp
// Register all implementations
builder.Services.AddScoped<INotificationSender, EmailNotificationSender>();
builder.Services.AddScoped<INotificationSender, SmsNotificationSender>();

// Inject all implementations
public class NotificationService
{
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationService(IEnumerable<INotificationSender> senders)
    {
        _senders = senders;
    }

    public async Task NotifyAllAsync(string message)
    {
        foreach (var sender in _senders)
        {
            await sender.SendAsync(message);
        }
    }
}
```

## Configuration Integration

Bind configuration to strongly-typed options:

```csharp
// appsettings.json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "Port": 587
  }
}

// EmailSettings.cs
public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int Port { get; set; }
}

// Program.cs
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

// Usage
public class EmailSender
{
    private readonly EmailSettings _settings;

    public EmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }
}
```

## Migration Strategy

For existing code without DI:

1. **Identify dependencies**: Find all `new` statements creating service instances
2. **Extract interfaces**: Create interfaces for concrete classes
3. **Add constructor parameters**: Accept dependencies through constructor
4. **Register services**: Add registrations in Program.cs
5. **Update tests**: Use mocks instead of real dependencies

## References

- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Dependency Injection Guidelines](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
