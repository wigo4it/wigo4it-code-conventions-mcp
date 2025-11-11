---
title: ADR 0002 - Use .NET Aspire for Web-Enabled Services
category: architecture
language: csharp
tags: [aspire, cloud-native, distributed-systems, apis, web-apps, observability]
status: Accepted
date: 2025-11-11
---

# ADR 0002: Use .NET Aspire for Web-Enabled Services

## Status

**Accepted**

## Context

Web-enabled services such as APIs and Web Apps are increasingly built as distributed systems that interact with multiple services, databases, caches, and external dependencies. Modern cloud-native applications require:

- **Service Discovery**: Dynamic discovery of services without hardcoding IP addresses and ports
- **Observability**: Deep insights through logs, metrics, and distributed tracing
- **Resilience**: Automatic retry logic and fault handling for spotty connections
- **Health Checks**: Monitoring application and dependency health
- **Telemetry**: OpenTelemetry integration for end-to-end tracing
- **Local Development Experience**: Efficient inner-loop development with orchestration and debugging capabilities

Traditionally, developers had to manually configure each of these features by reading complex documentation and figuring out the correct settings. This led to:

1. **Inconsistent implementations** across different projects
2. **Time-consuming setup** for best practices that were already available in .NET
3. **Poor developer experience** when managing multiple projects and services locally
4. **Lack of awareness** about features that had been shipped in previous .NET versions

.NET Aspire addresses these challenges by providing an opinionated cloud-native application stack that:

- Automates service discovery, configuration management, and dependency handling
- Provides NuGet packages for seamless integration with popular services (Redis, PostgreSQL, SQL Server, etc.)
- Includes built-in telemetry and health checks with recommended default settings
- Offers a web-based dashboard for real-time observability during development
- Uses a single line of code to enable best practices: `builder.AddServiceDefaults()`

## Decision

For web-enabled services (APIs, Web Apps, and other HTTP-based services) developed at Wigo4it, **the use of .NET Aspire is strongly recommended but not mandatory**.

### Recommended Use Cases

.NET Aspire should be used for:

1. **New Distributed Applications**: Any new web service that communicates with other services, databases, or external APIs
2. **Existing Applications with Multiple Dependencies**: Applications that integrate with Redis, SQL databases, message queues, or other backing services
3. **Microservices Architectures**: Systems composed of multiple interconnected services
4. **Applications Requiring Enhanced Observability**: Projects where logging, tracing, and metrics are critical
5. **Teams Seeking Improved Developer Experience**: Projects where local development orchestration and debugging are pain points

### What to Use from .NET Aspire

Developers have flexibility to adopt .NET Aspire incrementally:

- **Minimum Adoption**: Use the AppHost for orchestration of projects during local development
- **Recommended Adoption**: Add service defaults to enable telemetry, health checks, and resilience patterns
- **Full Adoption**: Use Aspire integrations for all external services, leverage service discovery, and use the dashboard in both development and production

### When Not to Use

.NET Aspire is not required for:

- Simple standalone applications with no external dependencies
- Console applications or batch jobs that don't communicate over HTTP
- Legacy applications where migration effort outweighs benefits
- Projects with existing robust orchestration and observability solutions

## Consequences

### Positive

1. **Standardized Best Practices**: All web services will benefit from Microsoft's recommended configurations for telemetry, resilience, and health checks
2. **Reduced Setup Time**: Single-line integration (`builder.AddServiceDefaults()`) enables multiple features automatically
3. **Improved Developer Productivity**: The AppHost and dashboard provide superior local development experience with orchestration and real-time insights
4. **Better Observability**: OpenTelemetry integration provides end-to-end tracing across the entire application and its dependencies
5. **Easier Service Integration**: Aspire integrations for popular services (Redis, SQL, etc.) come pre-configured with best practices
6. **Automatic Service Discovery**: No need to hardcode connection strings or endpoints
7. **Optional Deployment Benefits**: Teams can optionally leverage streamlined deployment to Azure Container Apps
8. **Backward Compatible**: Existing CI/CD pipelines and deployment processes remain unchanged unless teams choose to adopt Aspire deployment features

### Negative

1. **Learning Curve**: Developers need to understand Aspire concepts and project structure
2. **Additional Project**: Requires an AppHost project for orchestration (minimal overhead)
3. **Preview Status Considerations**: As of this ADR, .NET Aspire is in active development (check current status when implementing)
4. **Not Universally Applicable**: Some simple applications may not benefit from the added structure

### Neutral

1. **Flexibility**: Teams can choose how much of .NET Aspire to adopt (orchestration only, service defaults, full integration)
2. **No Forced Migration**: Existing connection strings and configurations can be retained
3. **IDE Support**: Both Visual Studio and Visual Studio Code (with C# Dev Kit) provide tooling to add Aspire with a few clicks

## Implementation Guidelines

### For New Projects

1. Start with the .NET Aspire Starter template to understand the structure
2. Create an AppHost project for orchestration
3. Add service defaults to all web projects
4. Use Aspire integrations for external services (databases, caches, etc.)
5. Leverage the dashboard during development

### For Existing Projects

1. Add .NET Aspire support via IDE tooling (right-click project → Add → .NET Aspire Orchestrator Support)
2. Create the AppHost project for orchestration
3. Add service defaults with a single line in `Program.cs`: `builder.AddServiceDefaults();`
4. Gradually migrate to Aspire integrations for external services as needed
5. Retain existing connection strings if desired (Aspire is additive, not prescriptive)

### Getting Started Resources

- [Official Documentation](https://docs.microsoft.com/dotnet/aspire)
- [Microsoft Learn Training](https://learn.microsoft.com/training/paths/dotnet-aspire/)
- [.NET Aspire Credential](https://aka.ms/aspire-credential)
- [eShop Reference Application](https://github.com/dotnet/eshop)
- [Aspire Samples Repository](https://github.com/dotnet/aspire-samples)
- [Beginner Video Series](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oXIKNExrtlVtVxy_G4tgzsc)

## Review

This decision should be reviewed:

- When .NET Aspire reaches a major version milestone (e.g., v2.0)
- If significant architectural patterns emerge that conflict with Aspire's approach
- After 12 months of adoption to assess developer feedback and productivity impact
- When alternative orchestration or observability solutions gain prominence in the .NET ecosystem

## Notes

- This ADR applies specifically to web-enabled services (APIs, Web Apps, gRPC services, etc.)
- For non-web applications (console apps, background workers), evaluate Aspire benefits on a case-by-case basis
- Teams are encouraged to share experiences and best practices as Aspire adoption grows
- The "strongly recommended" stance reflects Microsoft's push for Aspire as the standard approach for cloud-native .NET development
