---
title: ADR 0003 - Use OpenTelemetry for Observability
category: observability
language: csharp
tags: [opentelemetry, observability, tracing, metrics, logging, prometheus, grafana, monitoring]
status: Accepted
date: 2025-11-11
---

# ADR 0003: Use OpenTelemetry for Observability

## Status

**Accepted**

## Context

Modern distributed systems require comprehensive observability to understand system behavior, diagnose issues, and optimize performance. Traditional monitoring approaches often result in vendor lock-in and inconsistent instrumentation across services.

### Challenges We Face

1. **Distributed Tracing**: Understanding request flows across multiple services
2. **Metrics Collection**: Monitoring service health, performance, and resource usage
3. **Log Correlation**: Connecting logs to specific traces and operations
4. **Vendor Lock-in**: Being tied to specific APM (Application Performance Monitoring) vendors
5. **Inconsistent Instrumentation**: Different teams using different monitoring approaches
6. **Cost Management**: High costs associated with proprietary monitoring solutions
7. **Debugging Complexity**: Difficulty troubleshooting issues in production environments

### OpenTelemetry Overview

OpenTelemetry (OTel) is an open-source observability framework that provides:

- **Unified API**: Single API for traces, metrics, and logs (the three pillars of observability)
- **Vendor Neutrality**: Export to any backend (Prometheus, Grafana, Jaeger, Azure Monitor, etc.)
- **Automatic Instrumentation**: Built-in instrumentation for common libraries (ASP.NET Core, HttpClient, EF Core, etc.)
- **Standards-Based**: CNCF (Cloud Native Computing Foundation) incubating project with industry-wide adoption
- **Rich Ecosystem**: Extensive library support and active community
- **Future-Proof**: Industry standard being adopted by major vendors

## Decision

**All Wigo4it services and applications MUST use OpenTelemetry as the standard observability framework.**

### Core Requirements

1. **All Services Must Use OpenTelemetry**: Every service, API, and application must be instrumented
2. **Three Pillars**: Implement traces, metrics, and structured logs consistently
3. **Automatic Instrumentation First**: Use built-in instrumentation before creating custom spans
4. **Semantic Conventions**: Follow OpenTelemetry semantic conventions for attribute naming
5. **Decorate Operational Code**: All critical operations must have tracing and metrics
6. **Exporter Flexibility**: Support multiple exporters based on environment and requirements

### Recommended Exporters

#### Primary Recommendation: Prometheus + Grafana Stack

**Metrics**: Prometheus
- Industry-standard time-series database
- Pull-based model with service discovery
- Powerful query language (PromQL)
- Efficient storage and retention policies

**Visualization**: Grafana
- Rich dashboarding capabilities
- Alert management
- Multi-source data correlation
- Extensive plugin ecosystem

**Tracing**: Tempo (Grafana's tracing backend)
- Native integration with Grafana
- Cost-effective trace storage
- Compatible with OpenTelemetry Protocol (OTLP)

**Logs**: Loki (optional)
- Log aggregation with label-based indexing
- Seamless Grafana integration
- Correlation with traces and metrics

#### Alternative/Additional Exporters (Allowed)

The following exporters are permitted based on deployment context:

- **Azure Monitor / Application Insights**: For Azure-hosted services
- **AWS CloudWatch**: For AWS-hosted services
- **Jaeger**: Alternative for distributed tracing
- **Zipkin**: Legacy tracing systems
- **Elastic APM**: Existing Elastic Stack deployments
- **Console Exporter**: Development and debugging only
- **OTLP Exporter**: Generic OpenTelemetry Protocol export

#### Development Environment

- **Console Exporter**: Immediate feedback in development
- **OTLP to Local Collector**: Test full observability stack locally
- **.NET Aspire Dashboard**: Automatic if using Aspire (recommended)

## Implementation Requirements

### 1. NuGet Package Requirements

All services must include the following base packages:

```xml
<PackageReference Include="OpenTelemetry" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
```

**For ASP.NET Core Services**:
```xml
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
```

**For Services Using Entity Framework Core**:
```xml
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.12" />
```

**For Prometheus Export**:
```xml
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.9.0-beta.2" />
```

### 2. Service Configuration

All services must configure OpenTelemetry in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: builder.Configuration["ServiceName"] ?? "unknown-service",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
            serviceInstanceId: Environment.MachineName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (httpContext) =>
            {
                // Don't trace health checks
                return !httpContext.Request.Path.Value?.Contains("/health") ?? true;
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
        })
        .AddSource("MyCompany.*") // Add custom activity sources
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("MyCompany.*") // Add custom meters
        .AddPrometheusExporter());

var app = builder.Build();

// Expose Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

app.Run();
```

### 3. Decorating Operational Code with Tracing

All critical operations must be instrumented with custom spans:

#### Creating Activity Sources

Define activity sources at the class level:

```csharp
using System.Diagnostics;

public class OrderService
{
    private static readonly ActivitySource ActivitySource = new("MyCompany.OrderService");
    
    public async Task<Order> ProcessOrderAsync(string orderId)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder");
        activity?.SetTag("order.id", orderId);
        
        try
        {
            // Add tags for important information
            activity?.SetTag("order.status", "processing");
            
            var order = await _repository.GetOrderAsync(orderId);
            activity?.SetTag("order.total", order.TotalAmount);
            
            await ValidateOrderAsync(order);
            await ChargePaymentAsync(order);
            await ShipOrderAsync(order);
            
            activity?.SetTag("order.status", "completed");
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return order;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
    
    private async Task ValidateOrderAsync(Order order)
    {
        using var activity = ActivitySource.StartActivity("ValidateOrder");
        activity?.SetTag("order.id", order.Id);
        
        // Validation logic
        await Task.CompletedTask;
    }
}
```

#### Using Activity Events

Add events to provide additional context:

```csharp
public async Task<PaymentResult> ProcessPaymentAsync(decimal amount)
{
    using var activity = ActivitySource.StartActivity("ProcessPayment");
    activity?.SetTag("payment.amount", amount);
    
    activity?.AddEvent(new ActivityEvent("PaymentStarted"));
    
    var result = await _paymentGateway.ChargeAsync(amount);
    
    activity?.AddEvent(new ActivityEvent("PaymentCompleted", 
        tags: new ActivityTagsCollection
        {
            ["payment.transaction_id"] = result.TransactionId,
            ["payment.status"] = result.Status
        }));
    
    return result;
}
```

### 4. Decorating Code with Metrics

Create and use custom meters for business and operational metrics:

#### Defining Meters and Instruments

```csharp
using System.Diagnostics.Metrics;

public class OrderService
{
    private static readonly Meter Meter = new("MyCompany.OrderService");
    
    // Counter: Monotonically increasing value
    private static readonly Counter<long> OrdersProcessedCounter = 
        Meter.CreateCounter<long>("orders.processed", "orders", "Number of orders processed");
    
    // Histogram: Distribution of values
    private static readonly Histogram<double> OrderProcessingDuration = 
        Meter.CreateHistogram<double>("orders.processing.duration", "ms", "Order processing duration");
    
    // UpDownCounter: Value that can increase or decrease
    private static readonly UpDownCounter<int> ActiveOrders = 
        Meter.CreateUpDownCounter<int>("orders.active", "orders", "Number of active orders");
    
    // ObservableGauge: Current value at observation time
    private static readonly ObservableGauge<int> QueueDepth = 
        Meter.CreateObservableGauge("orders.queue.depth", 
            () => _orderQueue.Count, 
            "orders", 
            "Current order queue depth");
    
    public async Task<Order> ProcessOrderAsync(string orderId)
    {
        var stopwatch = Stopwatch.StartNew();
        ActiveOrders.Add(1);
        
        try
        {
            var order = await ProcessOrderInternalAsync(orderId);
            
            // Record metrics with tags
            OrdersProcessedCounter.Add(1, 
                new KeyValuePair<string, object?>("order.status", "success"),
                new KeyValuePair<string, object?>("order.type", order.Type));
            
            return order;
        }
        catch (Exception ex)
        {
            OrdersProcessedCounter.Add(1, 
                new KeyValuePair<string, object?>("order.status", "failed"),
                new KeyValuePair<string, object?>("error.type", ex.GetType().Name));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            OrderProcessingDuration.Record(stopwatch.ElapsedMilliseconds);
            ActiveOrders.Add(-1);
        }
    }
}
```

#### Metric Types and Usage

| Metric Type | Use Case | Example |
|-------------|----------|---------|
| **Counter** | Monotonically increasing values | Requests processed, errors occurred, items created |
| **Histogram** | Distribution of values | Request duration, payload size, query execution time |
| **UpDownCounter** | Values that increase/decrease | Active connections, queue depth, cache size |
| **ObservableGauge** | Point-in-time observations | Memory usage, CPU usage, thread count |

### 5. Semantic Conventions

Follow OpenTelemetry semantic conventions for attribute naming:

#### HTTP Attributes
```csharp
activity?.SetTag("http.method", "POST");
activity?.SetTag("http.url", request.Url);
activity?.SetTag("http.status_code", 200);
activity?.SetTag("http.user_agent", request.UserAgent);
```

#### Database Attributes
```csharp
activity?.SetTag("db.system", "postgresql");
activity?.SetTag("db.name", "orders");
activity?.SetTag("db.statement", query);
activity?.SetTag("db.operation", "SELECT");
```

#### Message Queue Attributes
```csharp
activity?.SetTag("messaging.system", "rabbitmq");
activity?.SetTag("messaging.destination", "order-queue");
activity?.SetTag("messaging.operation", "publish");
```

#### Custom Business Attributes
Use namespaced attributes for business-specific data:
```csharp
activity?.SetTag("mycompany.order.id", orderId);
activity?.SetTag("mycompany.customer.tier", "premium");
activity?.SetTag("mycompany.payment.method", "creditcard");
```

### 6. Structured Logging Integration

Correlate logs with traces using logging extensions:

```csharp
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
});
```

Log messages will automatically include trace context:

```csharp
_logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", 
    orderId, customerId);
// Automatically includes: TraceId, SpanId, TraceFlags
```

### 7. Exception Handling and Recording

Always record exceptions in spans:

```csharp
try
{
    await RiskyOperationAsync();
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    activity?.RecordException(ex);
    _logger.LogError(ex, "Operation failed for {EntityId}", entityId);
    throw;
}
```

### 8. Configuration Best Practices

#### Environment-Specific Configuration

**appsettings.Development.json**:
```json
{
  "OpenTelemetry": {
    "ServiceName": "MyService",
    "Exporters": {
      "Console": {
        "Enabled": true
      },
      "OTLP": {
        "Enabled": false
      }
    }
  }
}
```

**appsettings.Production.json**:
```json
{
  "OpenTelemetry": {
    "ServiceName": "MyService",
    "Exporters": {
      "Console": {
        "Enabled": false
      },
      "OTLP": {
        "Enabled": true,
        "Endpoint": "http://otel-collector:4317"
      },
      "Prometheus": {
        "Enabled": true,
        "ScrapeEndpointPath": "/metrics"
      }
    }
  }
}
```

#### Sampling Configuration

For high-traffic services, configure sampling:

```csharp
.WithTracing(tracing => tracing
    .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% of traces
    // Or use parent-based sampling
    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.1)))
    // Additional configuration...
)
```

### 9. .NET Aspire Integration

For services using .NET Aspire, OpenTelemetry is automatically configured:

```csharp
// Service defaults include OpenTelemetry configuration
builder.AddServiceDefaults();

// Aspire automatically:
// - Configures OTLP exporter
// - Enables automatic instrumentation
// - Provides built-in dashboard
// - Sets up health checks with metrics
```

### 10. Grafana Dashboard Setup

#### Prometheus Data Source Configuration

1. Add Prometheus as data source in Grafana
2. Configure scrape targets in Prometheus:

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'dotnet-services'
    static_configs:
      - targets: ['service1:8080', 'service2:8080']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

#### Recommended Dashboard Panels

**Service Health Overview**:
- Request rate (requests/second)
- Error rate (errors/second)
- Request duration (p50, p95, p99)
- Active requests

**System Resources**:
- CPU usage
- Memory usage
- GC collections
- Thread pool usage

**Business Metrics**:
- Orders processed
- Payment success rate
- Queue depth
- Active sessions

**Example PromQL Queries**:
```promql
# Request rate
rate(http_server_requests_total[5m])

# Error rate
rate(http_server_requests_total{http_status_code=~"5.."}[5m])

# Request duration p95
histogram_quantile(0.95, rate(http_server_request_duration_milliseconds_bucket[5m]))

# Custom business metric
rate(orders_processed_total[5m])
```

## Consequences

### Positive

1. **Vendor Independence**: Can switch observability backends without code changes
2. **Comprehensive Observability**: Unified view of traces, metrics, and logs
3. **Automatic Instrumentation**: Built-in instrumentation for common libraries reduces manual work
4. **Industry Standard**: Following CNCF standard ensures long-term support
5. **Better Debugging**: Distributed tracing makes debugging complex flows easier
6. **Performance Insights**: Metrics and histograms reveal performance bottlenecks
7. **Cost Effective**: Open-source stack (Prometheus + Grafana) reduces licensing costs
8. **Correlation**: Automatic correlation between logs, traces, and metrics
9. **Aspire Integration**: Seamless integration with .NET Aspire development experience

### Negative

1. **Learning Curve**: Teams need to learn OpenTelemetry concepts and APIs
2. **Initial Setup Effort**: Requires infrastructure setup (Prometheus, Grafana, collectors)
3. **Performance Overhead**: Instrumentation adds slight performance overhead (typically <5%)
4. **Storage Requirements**: Metrics and traces require storage infrastructure
5. **Configuration Complexity**: Multiple exporters and options require careful configuration

### Neutral

1. **Flexibility**: Multiple exporter options provide flexibility but require decision-making
2. **Semantic Conventions**: Following conventions requires discipline but improves consistency
3. **Infrastructure Dependency**: Requires operational expertise for Prometheus/Grafana stack

## Implementation Checklist

For each service, ensure:

- [ ] OpenTelemetry NuGet packages are installed
- [ ] Tracing is configured with appropriate instrumentation
- [ ] Metrics are configured with Prometheus exporter
- [ ] Custom ActivitySource is created for service-specific spans
- [ ] Custom Meter is created for service-specific metrics
- [ ] All critical operations are decorated with spans
- [ ] All business metrics are instrumented
- [ ] Exceptions are properly recorded in spans
- [ ] Semantic conventions are followed for attribute naming
- [ ] Configuration supports multiple environments (dev/prod)
- [ ] Prometheus scrape endpoint is exposed (`/metrics`)
- [ ] Grafana dashboards are created for the service
- [ ] Logs include trace context (TraceId, SpanId)
- [ ] Health checks are exposed with metrics
- [ ] Documentation includes observability architecture

## Monitoring Strategy

### Key Metrics to Track

**Golden Signals** (Site Reliability Engineering):
1. **Latency**: How long requests take (p50, p95, p99)
2. **Traffic**: Request rate (requests per second)
3. **Errors**: Error rate and types
4. **Saturation**: Resource usage (CPU, memory, connections)

**RED Method** (Request-driven services):
1. **Rate**: Requests per second
2. **Errors**: Failed requests per second
3. **Duration**: Request duration distribution

**USE Method** (Resource-focused):
1. **Utilization**: % time resource is busy
2. **Saturation**: Queue depth, waiting requests
3. **Errors**: Error count

### Alerting Guidelines

Configure alerts in Grafana for:
- Error rate exceeds threshold (e.g., >1% for 5 minutes)
- Request duration p99 exceeds SLA (e.g., >500ms)
- Service availability drops below target (e.g., <99.9%)
- Queue depth exceeds capacity (e.g., >1000 items)
- Resource utilization critical (e.g., >90% CPU)

## Resources

### Official Documentation
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)
- [.NET Observability with OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)

### Prometheus & Grafana
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [PromQL Cheat Sheet](https://promlabs.com/promql-cheat-sheet/)

### Aspire Integration
- [.NET Aspire Telemetry](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/telemetry)
- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard)

### Tools
- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
- [Jaeger Tracing](https://www.jaegertracing.io/)
- [Grafana Tempo](https://grafana.com/oss/tempo/)

## Review

This decision should be reviewed:

- When OpenTelemetry reaches a major version milestone (e.g., v2.0)
- After 6 months of production use to assess overhead and value
- When significant new observability features become available
- If alternative industry standards emerge
- When team feedback indicates implementation challenges

## Examples

### Complete Service Example

```csharp
// Program.cs
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("OrderService", serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("OrderService")
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("OrderService")
        .AddPrometheusExporter());

builder.Services.AddSingleton<OrderProcessor>();

var app = builder.Build();
app.MapPrometheusScrapingEndpoint();
app.MapPost("/orders", async (Order order, OrderProcessor processor) =>
{
    var result = await processor.ProcessOrderAsync(order);
    return Results.Ok(result);
});
app.Run();

// OrderProcessor.cs
public class OrderProcessor
{
    private static readonly ActivitySource ActivitySource = new("OrderService");
    private static readonly Meter Meter = new("OrderService");
    
    private static readonly Counter<long> OrdersProcessed = 
        Meter.CreateCounter<long>("orders.processed");
    private static readonly Histogram<double> ProcessingDuration = 
        Meter.CreateHistogram<double>("orders.processing.duration", "ms");
    
    public async Task<OrderResult> ProcessOrderAsync(Order order)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder");
        activity?.SetTag("order.id", order.Id);
        activity?.SetTag("order.amount", order.Amount);
        
        var sw = Stopwatch.StartNew();
        try
        {
            // Business logic here
            await Task.Delay(100); // Simulate work
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            OrdersProcessed.Add(1, 
                new KeyValuePair<string, object?>("status", "success"));
            
            return new OrderResult { Success = true };
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            OrdersProcessed.Add(1, 
                new KeyValuePair<string, object?>("status", "failed"));
            throw;
        }
        finally
        {
            sw.Stop();
            ProcessingDuration.Record(sw.ElapsedMilliseconds);
        }
    }
}
```

## Summary

OpenTelemetry provides a vendor-neutral, industry-standard approach to observability. By requiring all services to implement OpenTelemetry with Prometheus and Grafana as the primary stack, we ensure consistent, comprehensive observability across all Wigo4it applications while maintaining flexibility to use alternative exporters when needed. All operational code must be properly instrumented with traces and metrics to enable effective monitoring, debugging, and performance optimization.
