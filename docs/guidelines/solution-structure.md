# Solution and Project Structure Guidelines

Language: C#
Category: Architecture
Tags: solution-structure, project-organization, aspire, modules, domains

## Overview

This guideline describes the recommended structure for organizing solutions, projects, and files in Wigo4it software systems. A well-organized solution structure improves maintainability, enables modularity, and supports independent deployment of services.

## Guiding Principles

1. **Domain-Driven Organization**: Organize code by business domains or functional modules
2. **Clear Separation of Concerns**: Separate abstractions from implementations
3. **Testability**: Every module has corresponding unit tests
4. **Data Access Abstraction**: Repository patterns with explicit storage technology naming
5. **Aspire Integration**: Web-enabled projects use .NET Aspire for orchestration and observability

## Solution Structure

### Basic Structure

```
Wigo4it.{ProductName}/
├── Aspire/                                    # Aspire orchestration projects
│   ├── Wigo4it.{ProductName}.AppHost/        # Aspire App Host
│   └── Wigo4it.{ProductName}.ServiceDefaults/ # Shared service defaults
├── {DomainName}/                              # Domain/Module folder
│   ├── Wigo4it.{ProductName}.{DomainName}/   # Core domain logic
│   ├── Wigo4it.{ProductName}.{DomainName}.Abstractions/  # DTOs and interfaces
│   ├── Wigo4it.{ProductName}.{DomainName}.Data.{StorageType}/  # Data access
│   ├── Wigo4it.{ProductName}.{DomainName}.Api/  # API project (optional)
│   └── Wigo4it.{ProductName}.{DomainName}.Tests/  # Unit tests
├── {AnotherDomain}/
│   └── ...
└── Wigo4it.{ProductName}.sln                  # Solution file
```

### Example: E-Commerce Solution

```
Wigo4it.Inclusio/
├── Aspire/
│   ├── Wigo4it.Inclusio.AppHost/
│   └── Wigo4it.Inclusio.ServiceDefaults/
├── Catalog/
│   ├── Wigo4it.Inclusio.Catalog/
│   ├── Wigo4it.Inclusio.Catalog.Abstractions/
│   ├── Wigo4it.Inclusio.Catalog.Data.CosmosDb/
│   ├── Wigo4it.Inclusio.Catalog.Api/
│   └── Wigo4it.Inclusio.Catalog.Tests/
├── Inventory/
│   ├── Wigo4it.Inclusio.Inventory/
│   ├── Wigo4it.Inclusio.Inventory.Abstractions/
│   ├── Wigo4it.Inclusio.Inventory.Data.SqlServer/
│   └── Wigo4it.Inclusio.Inventory.Tests/
├── Persons/
│   ├── Wigo4it.Inclusio.Persons/
│   ├── Wigo4it.Inclusio.Persons.Abstractions/
│   ├── Wigo4it.Inclusio.Persons.Data.MongoDb/
│   └── Wigo4it.Inclusio.Persons.Tests/
└── Wigo4it.Inclusio.sln
```

## Project Types and Naming Conventions

### 1. Aspire Folder (Web-Enabled Projects Only)

For web applications, web APIs, or any project requiring orchestration and observability:

**Location**: `Aspire/` folder at the solution root

**Projects**:
- `Wigo4it.{ProductName}.AppHost` - Aspire orchestration host
- `Wigo4it.{ProductName}.ServiceDefaults` - Shared service configuration (telemetry, health checks, etc.)

**Example**:
```
Aspire/
├── Wigo4it.Inclusio.AppHost/
│   └── Program.cs                    # Orchestrates all services
└── Wigo4it.Inclusio.ServiceDefaults/
    └── Extensions.cs                 # Shared OpenTelemetry, health checks
```

**When to Use**:
- Web applications (Blazor, MVC, Razor Pages)
- Web APIs (REST, gRPC, SignalR)
- Background services that need orchestration
- Microservices architectures

### 2. Domain/Module Core Project

**Naming**: `Wigo4it.{ProductName}.{DomainName}`

**Purpose**: Contains core business logic, domain entities, business rules, and service implementations

**Contains**:
- Domain entities
- Business logic services
- Domain-specific exceptions
- Internal models
- Validation logic

**Example**:
```csharp
// Wigo4it.Inclusio.Persons/Person.cs
public class Person
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public Email EmailAddress { get; init; }
}

// Wigo4it.Inclusio.Persons/PersonService.cs
public class PersonService : IPersonService
{
    private readonly IPersonRepository _repository;
    
    public PersonService(IPersonRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Person> CreatePersonAsync(CreatePersonRequest request)
    {
        // Business logic here
    }
}
```

### 3. Abstractions Project

**Naming**: `Wigo4it.{ProductName}.{DomainName}.Abstractions`

**Purpose**: Contains DTOs (Data Transfer Objects), interfaces, and contracts that can be shared across projects without creating coupling

**Contains**:
- DTOs (as C# records)
- Service interfaces
- Repository interfaces
- Request/Response models
- Enums and constants

**Example**:
```csharp
// Wigo4it.Inclusio.Persons.Abstractions/Dtos/PersonDto.cs
public record PersonDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);

// Wigo4it.Inclusio.Persons.Abstractions/CreatePersonRequest.cs
public record CreatePersonRequest(
    string FirstName,
    string LastName,
    string Email
);

// Wigo4it.Inclusio.Persons.Abstractions/IPersonService.cs
public interface IPersonService
{
    Task<PersonDto> GetPersonAsync(Guid id);
    Task<PersonDto> CreatePersonAsync(CreatePersonRequest request);
}

// Wigo4it.Inclusio.Persons.Abstractions/IPersonRepository.cs
public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid id);
    Task<Person> AddAsync(Person person);
}
```

**Benefits**:
- API projects can reference abstractions without pulling in implementation details
- Enables clean architecture and dependency inversion
- Reduces coupling between projects

### 4. Data Access Project

**Naming**: `Wigo4it.{ProductName}.{DomainName}.Data.{StorageType}`

**Purpose**: Implements repository interfaces for specific storage technologies

**Storage Type Examples**:
- `MongoDb` - MongoDB databases
- `CosmosDb` - Azure Cosmos DB
- `SqlServer` - Microsoft SQL Server
- `PostgreSql` - PostgreSQL
- `TableStorage` - Azure Table Storage
- `Redis` - Redis cache
- `InMemory` - In-memory storage (for testing)

**Contains**:
- Repository implementations
- Database context classes
- Entity configurations
- Migrations (for SQL databases)
- Connection/client setup

**Example**:
```csharp
// Wigo4it.Inclusio.Persons.Data.MongoDb/PersonRepository.cs
public class PersonRepository : IPersonRepository
{
    private readonly IMongoCollection<PersonDocument> _collection;
    
    public PersonRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PersonDocument>("persons");
    }
    
    public async Task<Person?> GetByIdAsync(Guid id)
    {
        var document = await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
            
        return document?.ToPerson();
    }
}

// Wigo4it.Inclusio.Persons.Data.SqlServer/PersonRepository.cs
public class PersonRepository : IPersonRepository
{
    private readonly PersonDbContext _context;
    
    public PersonRepository(PersonDbContext context)
    {
        _context = context;
    }
    
    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _context.Persons
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
```

**Dependency Injection Registration**:
```csharp
// Wigo4it.Inclusio.Persons.Data.MongoDb/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersonsMongoDb(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IMongoClient>(sp => 
            new MongoClient(connectionString));
        services.AddScoped<IPersonRepository, PersonRepository>();
        return services;
    }
}
```

### 5. API Project (Optional)

**Naming**: `Wigo4it.{ProductName}.{DomainName}.Api`

**Purpose**: Provides a RESTful API or gRPC service for the domain, making it a truly independent microservice

**Contains**:
- API controllers or endpoints
- API-specific DTOs (if needed)
- Authentication/authorization
- API documentation (Swagger/OpenAPI)
- Health checks

**When to Create**:
- Domain needs to be independently deployable
- External systems need to access domain functionality
- Building a microservices architecture
- Domain has its own bounded context

**Example**:
```csharp
// Wigo4it.Inclusio.Persons.Api/Controllers/PersonsController.cs
[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;
    
    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PersonDto>> GetPerson(Guid id)
    {
        var person = await _personService.GetPersonAsync(id);
        return person is null ? NotFound() : Ok(person);
    }
    
    [HttpPost]
    public async Task<ActionResult<PersonDto>> CreatePerson(CreatePersonRequest request)
    {
        var person = await _personService.CreatePersonAsync(request);
        return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
    }
}

// Wigo4it.Inclusio.Persons.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register domain services
builder.Services.AddPersonsServices();
builder.Services.AddPersonsMongoDb(builder.Configuration.GetConnectionString("PersonsDb"));

var app = builder.Build();

// Configure Aspire
app.MapDefaultEndpoints();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await app.Run();
```

### 6. Unit Test Project

**Naming**: `Wigo4it.{ProductName}.{DomainName}.Tests`

**Purpose**: Contains unit tests for the domain logic

**Framework**: xUnit (required)

**Contains**:
- Unit tests for services
- Unit tests for domain logic
- Test fixtures
- Mock setups
- Test data builders

**Example**:
```csharp
// Wigo4it.Inclusio.Persons.Tests/PersonServiceTests.cs
public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _repositoryMock;
    private readonly PersonService _sut;
    
    public PersonServiceTests()
    {
        _repositoryMock = new Mock<IPersonRepository>();
        _sut = new PersonService(_repositoryMock.Object);
    }
    
    [Fact]
    public async Task CreatePersonAsync_WithValidRequest_ShouldCreatePerson()
    {
        // Arrange
        var request = new CreatePersonRequest(
            "John",
            "Doe",
            "john.doe@example.com"
        );
        
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Person>()))
            .ReturnsAsync((Person p) => p);
        
        // Act
        var result = await _sut.CreatePersonAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Person>()), Times.Once);
    }
}
```

**Testing Guidelines**:
- Every domain project must have a corresponding test project
- Aim for high code coverage (>80%)
- Use AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Use descriptive test names

## Project Dependencies

### Allowed Dependencies

```
API Project
└── References: Abstractions, Core, Data.{StorageType}

Core Project
└── References: Abstractions

Abstractions Project
└── References: None (or minimal shared libraries)

Data.{StorageType} Project
└── References: Abstractions, Core

Tests Project
└── References: Core, Abstractions, (optional: Data for integration tests)
```

### Dependency Rules

1. **Abstractions should be standalone**: Minimal dependencies, contains only contracts
2. **Core references Abstractions**: Business logic depends on abstractions, not implementations
3. **Data projects reference Core and Abstractions**: Implement repository interfaces
4. **API references all**: Orchestrates and exposes functionality
5. **Tests reference what they test**: Typically Core and Abstractions

## Aspire Configuration

### AppHost Project Structure

```csharp
// Wigo4it.Inclusio.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Add infrastructure
var mongoDb = builder.AddMongoDB("mongodb")
    .WithMongoExpress();

var personsDb = mongoDb.AddDatabase("personsDb");

var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var inventoryDb = sqlServer.AddDatabase("inventoryDb");

// Add domain APIs
var personsApi = builder.AddProject<Projects.Wigo4it_Inclusio_Persons_Api>("persons-api")
    .WithReference(personsDb);

var inventoryApi = builder.AddProject<Projects.Wigo4it_Inclusio_Inventory_Api>("inventory-api")
    .WithReference(inventoryDb);

var catalogApi = builder.AddProject<Projects.Wigo4it_Inclusio_Catalog_Api>("catalog-api");

// Add web frontend
builder.AddProject<Projects.Wigo4it_Inclusio_Web>("web")
    .WithReference(personsApi)
    .WithReference(inventoryApi)
    .WithReference(catalogApi);

await builder.Build().RunAsync();
```

### ServiceDefaults Project Structure

```csharp
// Wigo4it.Inclusio.ServiceDefaults/Extensions.cs
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
```

## Benefits of This Structure

1. **Modularity**: Each domain is self-contained and can be developed independently
2. **Testability**: Clear separation makes unit testing straightforward
3. **Flexibility**: Easy to swap storage technologies by creating new Data.{StorageType} projects
4. **Independent Deployment**: Optional API projects enable microservices architecture
5. **Observability**: Aspire provides built-in telemetry, logging, and health checks
6. **Scalability**: Domains can be scaled independently when deployed as separate services
7. **Clean Dependencies**: Abstractions prevent tight coupling between layers

## Anti-Patterns to Avoid

❌ **Don't**: Place all code in a single project
❌ **Don't**: Reference Data projects from Abstractions
❌ **Don't**: Skip the Abstractions project and reference Core directly from API
❌ **Don't**: Mix multiple domains in one project
❌ **Don't**: Skip unit tests
❌ **Don't**: Use generic names like "Common" or "Shared" for domain folders
❌ **Don't**: Put Aspire projects inside domain folders

## Migration Path

If you have an existing project that doesn't follow this structure:

1. **Create domain folders** for each functional area
2. **Extract abstractions** into separate projects
3. **Add Aspire** orchestration for web-enabled projects
4. **Create test projects** for each domain
5. **Split data access** into technology-specific projects
6. **Consider API projects** for domains that need independent deployment

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Clean Architecture Principles](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [xUnit Documentation](https://xunit.net/)
- See also: [ADR-0002: Use Aspire for Web-Enabled Services](../adr/0002-use-aspire-for-web-enabled-services.md)
