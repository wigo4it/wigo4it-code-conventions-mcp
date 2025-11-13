---
title: Fully Embrace Aspire for Orchestration and Clients
category: Recommendations
status: Active
last-updated: 2025-11-13
applicability: Projects using .NET Aspire
related:
  - ADR-002-adoption-of-aspire-for-distributed-applications.md
  - Structures/dotnet-project-structure.md
---

# Recommendation: Fully Embrace Aspire

## Summary

For solutions that take advantage of .NET Aspire, we recommend fully embracing Aspire across local orchestration and client integration. If a dependency can be orchestrated with Aspire, or emulated within Aspire for development, do exactly that. When Aspire provides hosting packages and client packages for a service, prefer the Aspire client libraries over the service's native client libraries because they integrate better with Aspire's configuration, service discovery, health, and telemetry.

## Scope

This recommendation applies to:
- New or existing solutions that include an Aspire AppHost and ServiceDefaults
- Services and modules that can be wired through Aspire resources/components
- Local development environments that can benefit from emulators/containers

## Why

- Consistent configuration: Aspire centralizes connection strings, endpoints, and secrets.
- First-class observability: Aspire components wire up logging, metrics, and tracing out of the box.
- Service discovery and health: References from AppHost provide robust naming, health checks, and readiness.
- Faster onboarding and parity: Developers run the whole system from AppHost with emulated dependencies.
- Easier evolution: Infrastructure defined in code scales from local emulators to real cloud resources with minimal changes.

## Guidance

### 1) Orchestrate everything you can with Aspire

- If Aspire has a resource/component integration for a dependency (database, cache, message broker, storage, etc.), declare it in AppHost and reference it from projects.
- Prefer local emulators or containers through Aspire for development (e.g., Redis, PostgreSQL, SQL Server, MongoDB, RabbitMQ/Kafka, Azure Storage via Azurite, Service Bus emulator, Event Hubs emulator when available).
- Keep configuration in ServiceDefaults where appropriate (logging, OpenTelemetry, resilience, retries, timeouts).

Example (conceptual):
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var db = builder.AddPostgres("postgres").AddDatabase("ordersdb");

builder.AddProject<Projects.Company_Product_Orders_Api>("orders-api")
       .WithReference(cache)
       .WithReference(db);
```

### 2) Prefer Aspire client libraries over native clients

When an Aspire component provides a client package for a dependency, use it instead of the raw SDK:

- Azure Storage Blobs: use the Aspire client package for Blobs instead of `Azure.Storage.Blobs` directly.
- Redis: use Aspire's StackExchange.Redis component rather than the bare `StackExchange.Redis` client.
- PostgreSQL: use Aspire's Npgsql component rather than the bare `Npgsql` configuration.
- Azure Service Bus / Event Hubs: prefer Aspire components over the direct `Azure.Messaging.*` SDKs when available.

Benefits:
- Unified configuration sourcing from AppHost references and ServiceDefaults.
- Built-in health checks, diagnostics, and OpenTelemetry instrumentation.
- Consistent resilience policies (retries, timeouts, circuit breakers) aligned with solution defaults.

### 3) Expect a host package and a client package

- Hosting package: used by the API or worker service to expose HTTP/gRPC and adopt ServiceDefaults (telemetry, health, configuration).
- Client package: used by other services/modules to connect to the hosted service or external dependency using Aspire wiring.
- Prefer the Aspire client for inter-service communication when such a component exists; otherwise, follow native SDK best practices and keep configuration centralized.

### 4) Fallbacks and exceptions

- If no Aspire component exists for a dependency, use the native client library.
- Wrap configuration consistently (IOptions, named HttpClients, connection string providers) so you can switch to an Aspire component later without large refactors.
- Document the gap and consider contributing or tracking an Aspire component for the dependency.

## Minimal implementation checklist

- AppHost: declare resources (databases, caches, messaging, storage) and reference projects.
- ServiceDefaults: enable structured logging, OpenTelemetry traces/metrics, and resilience policies.
- API/Worker hosts: reference ServiceDefaults; expose health endpoints and adopt standardized telemetry.
- Clients: use Aspire client packages (components) for dependencies where available; otherwise, centralize configuration and HttpClient usage.
- Local dev: prefer emulators/containers via Aspire resources; avoid ad-hoc local installs.
- CI/CD: run integration tests against Aspire-provisioned containers/emulators where feasible.

## Examples of Aspire-first choices (non-exhaustive)

- Data: Aspire.Npgsql / Aspire.SqlServer / Aspire.MongoDB (components) over direct Npgsql/SqlClient/Mongo drivers
- Cache: Aspire.StackExchange.Redis over direct StackExchange.Redis
- Messaging: Aspire.RabbitMQ / Aspire.Kafka / Aspire.Azure.ServiceBus over direct clients
- Storage: Aspire.Azure.Storage.Blobs/Queues/Tables over direct Azure SDKs

Consult the Aspire integration gallery for the latest component names and availability.

## Risks and mitigations

- Risk: A needed feature is missing in an Aspire client.
  - Mitigation: Fall back to the native SDK for the specific feature while keeping the bulk configured through Aspire. Track the gap.
- Risk: Component availability lags behind the native SDK.
  - Mitigation: Abstract client usage behind interfaces so swapping later is low-risk.
- Risk: Local emulator behavior differs from cloud service.
  - Mitigation: Keep a smoke test suite that runs against both emulator and real service in a staging environment.

## Cross-references

- ADR-002: Adoption of Aspire for Distributed Application Development
- Structures: .NET Project Structure (placement of `AppHost` and `ServiceDefaults`)

## Status and review

- Status: Active
- Review cadence: Quarterly, or when new Aspire components become available
