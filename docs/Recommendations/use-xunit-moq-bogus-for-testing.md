---
title: Use xUnit, Moq, and Bogus for Testing
category: Recommendations
status: Active
last-updated: 2025-11-13
applicability: All test projects
tags: [testing, xunit, moq, bogus, unit-testing, mocking]
related:
  - StyleGuides/csharp-style-guide.md
  - ADR-006-adoption-of-cqrs-pattern.md
---

# Recommendation: Use xUnit, Moq, and Bogus for Testing

## Summary

For all test projects, we standardize on **xUnit** as the test framework, **Moq** for mocking interfaces and dependencies, and **Bogus** for generating realistic fake data. This provides consistency across our codebase, reduces the learning curve, and ensures everyone uses well-supported, performant testing tools. Use of other testing frameworks or mocking libraries requires explicit approval.

## Scope

This recommendation applies to:
- All unit test projects
- Integration test projects
- Any project containing automated tests
- Test utilities and test helpers

## Standard Testing Stack

### xUnit - Test Framework (REQUIRED)

**xUnit** is the REQUIRED test framework for all projects.

**Why xUnit:**
- Modern, extensible, and actively maintained
- Microsoft's framework of choice for .NET projects
- Excellent performance with parallel test execution
- Strong support for dependency injection in test fixtures
- Better isolation with test class per test method pattern
- Industry standard in the .NET ecosystem

**Key Features:**
- `[Fact]` for simple tests
- `[Theory]` with `[InlineData]` for parameterized tests
- `IClassFixture<T>` for shared setup across tests
- `ICollectionFixture<T>` for shared context across test classes
- Async test support
- Parallel execution by default

### Moq - Mocking Framework (ALLOWED)

**Moq** is the RECOMMENDED and ALLOWED library for mocking interfaces and dependencies.

**Why Moq:**
- Simple, fluent API that's easy to learn
- Excellent IntelliSense support
- Powerful verification capabilities
- Strong type safety
- Industry-standard with extensive documentation
- Active maintenance and community support

**Key Features:**
- Mock creation: `new Mock<IService>()`
- Setup methods: `.Setup(x => x.Method()).Returns(value)`
- Verification: `.Verify(x => x.Method(), Times.Once)`
- Argument matching: `It.IsAny<T>()`, `It.Is<T>(predicate)`
- Property mocking and callback support

### Bogus - Fake Data Generation (ALLOWED)

**Bogus** is the RECOMMENDED and ALLOWED library for generating realistic test data.

**Why Bogus:**
- Generates realistic, locale-aware fake data
- Fluent, easy-to-read API
- Deterministic data with seeding for reproducible tests
- Reduces boilerplate in test setup
- Extensive data types (names, addresses, emails, dates, etc.)
- Active development and community support

**Key Features:**
- Rule-based object generation
- Seed support for deterministic tests
- Extensive built-in data generators
- Custom rule creation
- Collection generation
- Nested object support

## Usage Guidelines

### xUnit Test Structure

#### Basic Facts

```csharp
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsOrderDto()
    {
        // Arrange
        var service = new OrderService();
        var request = new CreateOrderRequest("customer-123", []);

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("customer-123", result.CustomerId);
    }
}
```

#### Theories with InlineData

```csharp
public class CalculatorTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 10, 15)]
    [InlineData(-1, 1, 0)]
    public void Add_WithTwoNumbers_ReturnsSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }
}
```

#### Class Fixtures for Shared Setup

```csharp
public class DatabaseFixture : IDisposable
{
    public DatabaseFixture()
    {
        // Setup database
        Connection = new SqlConnection(ConnectionString);
        Connection.Open();
    }

    public SqlConnection Connection { get; }

    public void Dispose()
    {
        Connection?.Dispose();
    }
}

public class OrderRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetById_WithExistingOrder_ReturnsOrder()
    {
        // Test uses _fixture.Connection
    }
}
```

### Moq Mocking Patterns

#### Basic Mocking

```csharp
[Fact]
public async Task CreateOrder_CallsRepository()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    var mockLogger = new Mock<ILogger<OrderService>>();
    
    mockRepository
        .Setup(x => x.AddAsync(It.IsAny<Order>(), default))
        .Returns(Task.CompletedTask);

    var service = new OrderService(mockRepository.Object, mockLogger.Object);
    var command = new CreateOrderCommand("customer-123", []);

    // Act
    await service.CreateOrderAsync(command);

    // Assert
    mockRepository.Verify(
        x => x.AddAsync(It.IsAny<Order>(), default),
        Times.Once);
}
```

#### Returning Values

```csharp
[Fact]
public async Task GetOrder_ReturnsOrderFromRepository()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    var expectedOrder = new Order("order-123", "customer-123");

    mockRepository
        .Setup(x => x.GetByIdAsync("order-123", default))
        .ReturnsAsync(expectedOrder);

    var service = new OrderService(mockRepository.Object);

    // Act
    var result = await service.GetOrderAsync("order-123");

    // Assert
    Assert.Equal(expectedOrder, result);
}
```

#### Argument Matching

```csharp
[Fact]
public async Task CreateOrder_WithSpecificCustomer_SavesCorrectly()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    
    mockRepository
        .Setup(x => x.AddAsync(
            It.Is<Order>(o => o.CustomerId == "customer-123"),
            default))
        .Returns(Task.CompletedTask);

    var service = new OrderService(mockRepository.Object);

    // Act & Assert continues...
}
```

#### Exception Throwing

```csharp
[Fact]
public async Task CreateOrder_WhenRepositoryFails_ThrowsException()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    
    mockRepository
        .Setup(x => x.AddAsync(It.IsAny<Order>(), default))
        .ThrowsAsync(new DatabaseException("Connection failed"));

    var service = new OrderService(mockRepository.Object);

    // Act & Assert
    await Assert.ThrowsAsync<DatabaseException>(
        () => service.CreateOrderAsync(new CreateOrderCommand("customer-123", [])));
}
```

### Bogus Data Generation

#### Simple Object Generation

```csharp
[Fact]
public void TestWithGeneratedUser()
{
    // Arrange
    var faker = new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Guid().ToString())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
        .RuleFor(u => u.DateOfBirth, f => f.Date.Past(30, DateTime.Now.AddYears(-18)));

    var user = faker.Generate();

    // Act & Assert with generated user
}
```

#### Collection Generation

```csharp
[Fact]
public void TestWithMultipleOrders()
{
    // Arrange
    var orderFaker = new Faker<Order>()
        .RuleFor(o => o.Id, f => f.Random.Guid().ToString())
        .RuleFor(o => o.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(o => o.OrderDate, f => f.Date.Recent(30))
        .RuleFor(o => o.Total, f => f.Finance.Amount(10, 1000));

    var orders = orderFaker.Generate(10); // Generate 10 orders

    // Act & Assert with generated orders
}
```

#### Deterministic Data with Seeding

```csharp
[Fact]
public void TestWithDeterministicData()
{
    // Arrange - Same seed produces same data
    var faker = new Faker<Product>()
        .UseSeed(12345)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Price, f => f.Finance.Amount(1, 100));

    var product1 = faker.Generate();
    
    // Regenerate with same seed
    faker = new Faker<Product>()
        .UseSeed(12345)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Price, f => f.Finance.Amount(1, 100));
    
    var product2 = faker.Generate();

    // Assert - Both products are identical
    Assert.Equal(product1.Name, product2.Name);
    Assert.Equal(product1.Price, product2.Price);
}
```

#### Complex Object Graphs

```csharp
[Fact]
public void TestWithComplexOrderStructure()
{
    // Arrange
    var orderItemFaker = new Faker<OrderItem>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid().ToString())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
        .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(5, 100));

    var orderFaker = new Faker<Order>()
        .RuleFor(o => o.Id, f => f.Random.Guid().ToString())
        .RuleFor(o => o.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(o => o.Items, f => orderItemFaker.Generate(f.Random.Int(1, 5)))
        .RuleFor(o => o.OrderDate, f => f.Date.Recent(30));

    var order = orderFaker.Generate();

    // Act & Assert with complex order
}
```

## Combining xUnit, Moq, and Bogus

```csharp
public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<ILogger<CreateOrderHandler>> _mockLogger;
    private readonly Faker<CreateOrderRequest> _requestFaker;

    public CreateOrderHandlerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockLogger = new Mock<ILogger<CreateOrderHandler>>();
        
        // Setup Bogus faker for test data
        _requestFaker = new Faker<CreateOrderRequest>()
            .CustomInstantiator(f => new CreateOrderRequest(
                f.Random.Guid().ToString(),
                new List<OrderItemDto>
                {
                    new(f.Commerce.Product(), f.Random.Int(1, 5), f.Finance.Amount(10, 100))
                },
                new PaymentInfoDto("credit_card", f.Finance.CreditCardNumber())));
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_CreatesOrder()
    {
        // Arrange - Using Bogus for test data
        var request = _requestFaker.Generate();
        
        // Arrange - Using Moq for dependencies
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        var handler = new CreateOrderHandler(
            _mockRepository.Object,
            _mockLogger.Object);

        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Items,
            request.PaymentInfo);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert - Using xUnit assertions
        Assert.NotNull(result);
        Assert.Equal(request.CustomerId, result.CustomerId);
        
        // Assert - Using Moq verification
        _mockRepository.Verify(
            x => x.AddAsync(It.IsAny<Order>(), default),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task HandleAsync_WithVariousItemCounts_HandlesCorrectly(int itemCount)
    {
        // Arrange - Generate request with specific item count
        var items = new Faker<OrderItemDto>()
            .CustomInstantiator(f => new OrderItemDto(
                f.Commerce.Product(),
                f.Random.Int(1, 5),
                f.Finance.Amount(10, 100)))
            .Generate(itemCount);

        var request = new CreateOrderRequest(
            Guid.NewGuid().ToString(),
            items,
            new PaymentInfoDto("credit_card", "4111111111111111"));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        var handler = new CreateOrderHandler(
            _mockRepository.Object,
            _mockLogger.Object);

        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Items,
            request.PaymentInfo);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(itemCount, result.Items.Count);
    }
}
```

## Discouraged Frameworks

The following frameworks are **DISCOURAGED** and require explicit approval before use:

### NUnit (Discouraged)
- **Why discouraged**: While mature and capable, using NUnit alongside xUnit creates inconsistency
- **When to request approval**: Legacy projects already using NUnit, interoperability requirements
- **Approval required from**: Team Lead or Architect

### MSTest (Discouraged)
- **Why discouraged**: Less modern than xUnit, weaker extensibility, inconsistent with our standards
- **When to request approval**: Rare cases where Visual Studio Test integration is critical
- **Approval required from**: Team Lead or Architect

### NSubstitute (Discouraged)
- **Why discouraged**: Similar to Moq but different syntax creates inconsistency
- **When to request approval**: Team has strong NSubstitute expertise, specific feature requirement
- **Approval required from**: Team Lead

### AutoFixture (Discouraged)
- **Why discouraged**: More complex than Bogus, less intuitive API, harder to understand test data
- **When to request approval**: Specific auto-mocking scenarios, legacy code already using it
- **Approval required from**: Team Lead

### FakeItEasy (Discouraged)
- **Why discouraged**: Different API than Moq, creates inconsistency
- **When to request approval**: Specific features not available in Moq
- **Approval required from**: Team Lead

## Package References

Add these packages to your test projects:

```xml
<ItemGroup>
  <!-- Required: Test framework -->
  <PackageReference Include="xunit" Version="2.9.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  
  <!-- Required for code coverage -->
  <PackageReference Include="coverlet.collector" Version="6.0.2" />
  
  <!-- Recommended: Mocking framework -->
  <PackageReference Include="Moq" Version="4.20.70" />
  
  <!-- Recommended: Fake data generation -->
  <PackageReference Include="Bogus" Version="35.6.1" />
  
  <!-- Optional: For integration tests with ASP.NET Core -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
</ItemGroup>
```

## Best Practices

### Test Organization

1. **Arrange-Act-Assert Pattern**: Use AAA pattern consistently
2. **One Assertion Per Test**: Focus tests on single behaviors
3. **Descriptive Test Names**: Use `MethodName_Scenario_ExpectedBehavior` naming
4. **Test Class Per Class Under Test**: Mirror production code structure

### Mock Usage

1. **Mock Interfaces, Not Classes**: Only mock abstractions
2. **Verify Meaningful Interactions**: Don't over-verify
3. **Setup Only What's Needed**: Keep mocks focused
4. **Use Strict Mocks Sparingly**: Default loose behavior is usually sufficient

### Data Generation

1. **Use Bogus for Complex Data**: Don't manually create test data
2. **Seed for Deterministic Tests**: Use `.UseSeed()` when test reliability is critical
3. **Keep Fakers Reusable**: Define fakers once, reuse across tests
4. **Generate Realistic Data**: Use appropriate Bogus generators for domains

### Test Maintenance

1. **Extract Common Setup**: Use fixtures or helper methods
2. **Avoid Test Interdependence**: Each test should be independent
3. **Keep Tests Simple**: Tests should be easier to understand than production code
4. **Review Test Coverage**: Maintain high coverage but focus on meaningful tests

## Approval Process

If you need to use a framework not listed as required or allowed:

1. **Document the Reason**: Explain why the standard frameworks are insufficient
2. **Request Approval**: Contact your Team Lead or Architect
3. **Provide Justification**: Show specific features or requirements
4. **Consider Consistency**: Evaluate impact on team consistency
5. **Document Decision**: If approved, document in project README

## Benefits

- **Consistency**: All teams use the same testing tools
- **Knowledge Sharing**: Developers can easily work across projects
- **Onboarding**: New developers learn one testing stack
- **Tooling**: Standard tools work reliably across all projects
- **Community**: Extensive resources and support for standard tools
- **Maintenance**: Well-supported, actively maintained libraries

## References

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Bogus Documentation](https://github.com/bchavez/Bogus)
- [xUnit Best Practices](https://xunit.net/docs/comparisons)
- [Testing ASP.NET Core Applications](https://learn.microsoft.com/en-us/aspnet/core/test/)

## Related

- **ADR-006**: CQRS Pattern - Test handler implementations using these frameworks
- **C# Style Guide**: Follow style guidelines in test code
- **Project Structure**: Place test projects alongside code under test
