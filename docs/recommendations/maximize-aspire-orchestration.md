---
title: Maximize Aspire Orchestration and Use Aspire Client Libraries
category: aspire
language: csharp
tags: [aspire, orchestration, integrations, emulators, client-libraries, best-practices]
---

# Recommendation: Maximize Aspire Orchestration and Use Aspire Client Libraries

## Overview

For projects that adopt .NET Aspire, it is strongly recommended to **maximize the use of Aspire orchestration** for all external service dependencies and to **prefer Aspire-specific client libraries** over native client libraries.

## Guiding Principles

1. **Let Aspire Orchestrate Everything It Can**: If Aspire provides an integration for a service, use it
2. **Use Emulators When Available**: Prefer local emulators over requiring actual service infrastructure during development
3. **Prefer Aspire Client Libraries**: Use Aspire-provided client libraries instead of native SDKs when available
4. **Consistent Developer Experience**: Ensure all developers can run the entire application stack locally without external dependencies

## Orchestration Recommendations

### Storage Services

#### Databases

**SQL Server**
```csharp
// AppHost - Use SQL Server container or Azure SQL emulator
var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("mydb");

// Consumer Project - Use Aspire client library
builder.AddSqlServerClient("mydb");
```

**PostgreSQL**
```csharp
// AppHost - Use PostgreSQL container
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("mydb");

// Consumer Project - Use Aspire client library
builder.AddNpgsqlDataSource("mydb");
```

**MongoDB**
```csharp
// AppHost - Use MongoDB container
var mongo = builder.AddMongoDB("mongo")
    .WithDataVolume()
    .AddDatabase("mydb");

// Consumer Project - Use Aspire client library
builder.AddMongoDBClient("mongo");
```

**Azure Cosmos DB**
```csharp
// AppHost - Use Cosmos DB emulator
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator()
    .AddDatabase("mydb");

// Consumer Project - Use Aspire client library
builder.AddAzureCosmosDBClient("cosmos");
```

**MySQL**
```csharp
// AppHost - Use MySQL container
var mysql = builder.AddMySql("mysql")
    .WithDataVolume()
    .AddDatabase("mydb");

// Consumer Project - Use Aspire client library
builder.AddMySqlDataSource("mydb");
```

#### Caching and Key-Value Stores

**Redis**
```csharp
// AppHost - Use Redis container
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddRedisClient("redis");
// Or for distributed caching
builder.AddRedisDistributedCache("redis");
```

**Valkey (Redis alternative)**
```csharp
// AppHost - Use Valkey container
var valkey = builder.AddValkey("valkey")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddValkeyClient("valkey");
```

**Garnet (Microsoft's Redis alternative)**
```csharp
// AppHost - Use Garnet container
var garnet = builder.AddGarnet("garnet");

// Consumer Project - Use Aspire client library
builder.AddGarnetClient("garnet");
```

### Messaging and Event Streaming

**RabbitMQ**
```csharp
// AppHost - Use RabbitMQ container
var messaging = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddRabbitMQClient("messaging");
```

**Apache Kafka**
```csharp
// AppHost - Use Kafka container
var kafka = builder.AddKafka("kafka")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddKafkaProducer<string, MyMessage>("kafka");
builder.AddKafkaConsumer<string, MyMessage>("kafka", consumerBuilder => {
    consumerBuilder.Config.GroupId = "my-consumer-group";
});
```

**Azure Service Bus**
```csharp
// AppHost - Use Service Bus emulator when available
var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(); // Use emulator if available

// Consumer Project - Use Aspire client library
builder.AddAzureServiceBusClient("messaging");
```

**NATS**
```csharp
// AppHost - Use NATS container
var nats = builder.AddNats("nats");

// Consumer Project - Use Aspire client library
builder.AddNatsClient("nats");
```

### Search and Analytics

**Elasticsearch**
```csharp
// AppHost - Use Elasticsearch container
var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddElasticsearchClient("elasticsearch");
```

**Milvus (Vector Database)**
```csharp
// AppHost - Use Milvus container
var milvus = builder.AddMilvus("milvus")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddMilvusClient("milvus");
```

**Qdrant (Vector Database)**
```csharp
// AppHost - Use Qdrant container
var qdrant = builder.AddQdrant("qdrant")
    .WithDataVolume();

// Consumer Project - Use Aspire client library
builder.AddQdrantClient("qdrant");
```

### AI and Machine Learning

**Ollama (Local LLM)**
```csharp
// AppHost - Use Ollama container
var ollama = builder.AddOllama("ollama")
    .AddModel("llama3.2");

// Consumer Project - Use Aspire client library
builder.AddOllamaClientFromHttpClient("ollama");
```

**Azure OpenAI**
```csharp
// AppHost - Configure Azure OpenAI
var openai = builder.AddAzureOpenAI("openai");

// Consumer Project - Use Aspire client library
builder.AddAzureOpenAIClient("openai");
```

### Observability and Monitoring

**Seq (Structured Logging)**
```csharp
// AppHost - Use Seq container
var seq = builder.AddSeq("seq");

// Consumer Project - Aspire configures Seq automatically through service defaults
```

**Prometheus**
```csharp
// AppHost - Use Prometheus container
var prometheus = builder.AddPrometheus("prometheus");
```

**Grafana**
```csharp
// AppHost - Use Grafana container
var grafana = builder.AddGrafana("grafana");
```

## Benefits of Using Aspire Orchestration

### 1. **Simplified Local Development**

Developers can run the entire application stack locally without:
- Installing services manually
- Managing service versions
- Configuring connection strings
- Starting/stopping services manually

```csharp
// Everything runs with a single F5
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").AddDatabase("mydb");
var redis = builder.AddRedis("redis");
var rabbitmq = builder.AddRabbitMQ("messaging");
var seq = builder.AddSeq("seq");

builder.AddProject<Projects.MyApi>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq);
```

### 2. **Automatic Service Discovery**

No hardcoded connection strings or endpoints:

```csharp
// ❌ Don't do this
var connectionString = "Server=localhost,1433;Database=mydb;User Id=sa;Password=...";
var connection = new SqlConnection(connectionString);

// ✅ Do this - Aspire handles service discovery
builder.AddSqlServerClient("mydb");
// Connection string is automatically configured based on environment
```

### 3. **Built-in Health Checks**

Aspire integrations automatically register health checks:

```csharp
// AppHost
var postgres = builder.AddPostgres("postgres").AddDatabase("mydb");

// Consumer Project - health check is automatic
builder.AddNpgsqlDataSource("mydb");
// Health check for PostgreSQL is automatically registered
```

### 4. **Integrated Telemetry**

All Aspire client libraries include OpenTelemetry instrumentation:

- Database queries are traced
- Cache operations are logged
- Message publishing/consumption is tracked
- All telemetry appears in the Aspire dashboard

### 5. **Consistent Configuration Patterns**

All services use the same configuration approach:

```csharp
// AppHost - declare the service
var service = builder.AddServiceType("service-name");

// Consumer - reference the service
builder.AddServiceTypeClient("service-name");
```

### 6. **Environment Parity**

Same code works in all environments:

- **Development**: Uses containers/emulators orchestrated by Aspire
- **Testing**: Uses test containers or mocked services
- **Production**: Connects to actual cloud services (Azure, AWS, etc.)

## Benefits of Using Aspire Client Libraries

### 1. **Enhanced Telemetry**

Aspire client libraries provide richer telemetry than native SDKs:

```csharp
// Native MongoDB driver
builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(configuration.GetConnectionString("mongo")));

// ✅ Aspire MongoDB client - includes telemetry, health checks, service discovery
builder.AddMongoDBClient("mongo");
```

### 2. **Service Defaults Integration**

Aspire clients automatically integrate with service defaults:

- Resilience policies
- Health checks
- Distributed tracing
- Metrics collection

```csharp
// Consumer Project
builder.AddServiceDefaults(); // Configures defaults for all services
builder.AddRedisClient("redis"); // Automatically uses service defaults
```

### 3. **Simplified Configuration**

No need to manually configure connection strings:

```csharp
// ❌ Native client - manual configuration
builder.Services.Configure<RabbitMQOptions>(
    configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IConnection>(sp => {
    var options = sp.GetRequiredService<IOptions<RabbitMQOptions>>().Value;
    var factory = new ConnectionFactory {
        HostName = options.HostName,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password
    };
    return factory.CreateConnection();
});

// ✅ Aspire client - single line
builder.AddRabbitMQClient("messaging");
```

### 4. **Future-Proof**

Aspire clients are maintained by Microsoft and the community:

- Security updates
- Performance improvements
- New features
- Breaking change management

## Implementation Strategy

### For New Projects

1. **Identify all external dependencies** (databases, caches, message queues, AI services)
2. **Check Aspire integration catalog** at [https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview)
3. **Use Aspire orchestration and client libraries** for all available integrations
4. **Configure emulators** for services that support them (Cosmos DB, Azure Storage, etc.)
5. **Use containers** for services without emulators (PostgreSQL, Redis, etc.)

### For Existing Projects

1. **Audit current dependencies**:
   ```bash
   # List all NuGet packages
   dotnet list package
   ```

2. **Identify Aspire equivalents**:
   - `MongoDB.Driver` → `Aspire.MongoDB.Driver`
   - `StackExchange.Redis` → `Aspire.StackExchange.Redis`
   - `Npgsql` → `Aspire.Npgsql`
   - `Azure.Storage.Blobs` → `Aspire.Azure.Storage.Blobs`

3. **Migrate incrementally**:
   - Start with one service at a time
   - Update AppHost to orchestrate the service
   - Replace native client with Aspire client
   - Verify telemetry and health checks work
   - Remove old configuration code

4. **Test thoroughly**:
   - Verify local development works
   - Ensure production deployments are unaffected
   - Check that existing connection strings still work (if needed)

## When to Use Native Clients

There are limited cases where native clients are acceptable:

1. **Aspire integration doesn't exist yet**: Check the [Aspire Community Toolkit](https://github.com/CommunityToolkit/Aspire) first
2. **Specialized features not exposed by Aspire client**: Rare, but possible
3. **Third-party library requires native client**: Some frameworks mandate specific client types
4. **Performance-critical scenarios with proven bottlenecks**: Profile first, optimize second

Even in these cases, consider:
- Wrapping the native client with telemetry
- Registering manual health checks
- Using Aspire orchestration even if not using the client library

## Available Aspire Integrations

As of .NET Aspire 9.0, integrations include:

**Databases**: SQL Server, PostgreSQL, MySQL, MongoDB, Oracle, Azure Cosmos DB  
**Caching**: Redis, Valkey, Garnet  
**Messaging**: RabbitMQ, Kafka, Azure Service Bus, NATS  
**Storage**: Azure Blob Storage, Azure Table Storage, Azure Queue Storage  
**Search**: Elasticsearch, Milvus, Qdrant  
**AI**: Azure OpenAI, Ollama  
**Observability**: Seq, Prometheus, Grafana, OpenTelemetry  
**Identity**: Azure Key Vault, Azure Active Directory  

Full list: [https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview)

## Checklist for Aspire-Based Projects

- [ ] All databases are orchestrated by Aspire (containers or emulators)
- [ ] All caches are orchestrated by Aspire
- [ ] All message queues are orchestrated by Aspire
- [ ] All storage services use Aspire integrations where available
- [ ] All AI services use Aspire integrations (Azure OpenAI, Ollama)
- [ ] Consumer projects use Aspire client libraries, not native SDKs
- [ ] Emulators are used for Azure services (Cosmos DB, Storage, Service Bus)
- [ ] Data volumes are configured for stateful services (`WithDataVolume()`)
- [ ] Service discovery is enabled (no hardcoded connection strings in code)
- [ ] Telemetry from all services appears in Aspire dashboard
- [ ] Health checks are automatically registered for all dependencies
- [ ] Local development requires only `dotnet run` on AppHost project

## Resources

- [Aspire Integrations Catalog](https://learn.microsoft.com/dotnet/aspire/fundamentals/integrations-overview)
- [Aspire Community Toolkit](https://github.com/CommunityToolkit/Aspire)
- [Building Aspire Integrations](https://learn.microsoft.com/dotnet/aspire/extensibility/custom-integration)
- [Aspire Emulators Guide](https://learn.microsoft.com/dotnet/aspire/emulators-overview)
- [Service Discovery in Aspire](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview)

## Summary

**Maximize Aspire orchestration** by having Aspire manage all possible external dependencies through containers or emulators. **Use Aspire-specific client libraries** instead of native SDKs to gain automatic telemetry, health checks, resilience, and service discovery. This approach delivers a superior developer experience and ensures consistency across all environments.
