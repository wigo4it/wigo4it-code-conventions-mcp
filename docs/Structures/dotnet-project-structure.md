---
title: .NET Project Structure
category: Structures
status: Active
last-updated: 2025-11-13
applicability: .NET 10 / C# 14
tags: [dotnet, project-structure, organization, aspire, modular-monolith]
---

# .NET Project Structure

## Overview

All .NET projects are stored under the `src/` folder of the repository. The `src/` folder contains the solution file and all project folders. Projects are organized by module/domain to favor modular monolith design (see ADR-003). Shared libraries that are consumed by multiple modules live at the root of `src/`.

If the solution is locally orchestrated using Aspire, the AppHost and ServiceDefaults are stored in a separate `Aspire/` folder under `src/`.

## Naming conventions

For a product named `Company.Product` and a module/domain named `{Module}`:

- Core module project: `Company.Product.{Module}`
- Abstractions project: `Company.Product.{Module}.Abstractions`
- Data project(s): `Company.Product.{Module}.Data.{StorageTechnology}` (optional; 0..n)
- Test project: `Company.Product.{Module}.Tests` (xUnit)
- API host (when separately hosted as a service): `Company.Product.{Module}.Api` (optional)

Shared libraries at the root of `src/`:

- Shared core library: `Company.Product.Core`
- Shared integration events: `Company.Product.IntegrationEvents`

Namespaces should follow project names and use file-scoped namespaces.

## Folder layout

Top-level layout:

```
repo-root/
src/
├─ Company.Product.sln
├─ Aspire/                           # Optional: local orchestration with Aspire
│  ├─ Company.Product.AppHost/       # AppHost project
│  └─ Company.Product.ServiceDefaults/ # ServiceDefaults shared config
├─ Core/                              # Shared libraries used across modules
│  ├─ Company.Product.Core/
│  └─ Company.Product.IntegrationEvents/
├─ {ModuleA}/                         # One folder per module/domain
│  ├─ Company.Product.{ModuleA}/
│  ├─ Company.Product.{ModuleA}.Abstractions/
│  ├─ Company.Product.{ModuleA}.Data.{StorageTechnology}/   # optional (0..n)
│  ├─ Company.Product.{ModuleA}.Api/                        # optional (hosted service)
│  └─ Company.Product.{ModuleA}.Tests/                      # xUnit tests
└─ {ModuleB}/
   ├─ Company.Product.{ModuleB}/
   ├─ Company.Product.{ModuleB}.Abstractions/
   ├─ Company.Product.{ModuleB}.Data.{StorageTechnology}/   # optional
   ├─ Company.Product.{ModuleB}.Api/                        # optional
   └─ Company.Product.{ModuleB}.Tests/
```

Notes:
- The `.sln` file resides in `src/` and references all projects.
- Each module/domain gets its own folder directly under `src/`.
- Shared libraries that are used by multiple modules are placed in a `Core/` area under `src/` (or directly under `src/` if preferred). Keep shared code minimal and intentional.
- Aspire projects live under `src/Aspire/` to separate orchestration concerns from module code.

## Module folder content

Each module/domain folder contains the following projects:

1. `Company.Product.{Module}`
   - Purpose: Core module/domain logic (domain model, application services, commands/queries)
   - Internal visibility: Keep internals hidden; expose only through Abstractions

2. `Company.Product.{Module}.Abstractions`
   - Purpose: Public contracts for the module (interfaces, DTOs, events)
   - Dependency direction: Other modules depend on Abstractions, not the core

3. `Company.Product.{Module}.Data.{StorageTechnology}` (optional; 0..n)
   - Purpose: Persistence implementation(s) for the module (e.g., `SqlServer`, `Postgres`, `Mongo`, `EventStore`)
   - Guidance: One DbContext per module when using EF Core; avoid cross-module joins

4. `Company.Product.{Module}.Api` (optional)
   - Purpose: ASP.NET Core host exposing the module as a separate service (HTTP/gRPC)
   - Use only when the module must be independently hosted/deployed

5. `Company.Product.{Module}.Tests`
   - Purpose: xUnit test project covering unit, integration, and contract tests for the module
   - Guidance: Favor tests at the module boundary; include architecture tests to enforce boundaries

## Shared libraries

- `Company.Product.Core` should contain general-purpose primitives and cross-cutting concerns that are truly shared (e.g., base result types, error handling abstractions). Keep lean to avoid coupling.
- `Company.Product.IntegrationEvents` contains integration event contracts shared across modules/services. Version carefully and avoid implementation details.

## Aspire orchestration (optional)

When using Aspire for local orchestration:

- Place the AppHost and ServiceDefaults in `src/Aspire/` as separate projects:
  - `Company.Product.AppHost`
  - `Company.Product.ServiceDefaults`
- AppHost wires up projects/resources and references module API hosts as needed.
- ServiceDefaults holds cross-cutting configuration for logging, metrics, tracing, etc.
- See ADR-002 for rationale and broader guidance on Aspire adoption.

## Solution and project references

- The solution file (`src/Company.Product.sln`) includes all module projects, shared libraries, and Aspire projects (if present).
- Reference flow:
  - `{Module}.Api` references `{Module}` and `{Module}.Abstractions` (and data project if applicable)
  - `{Module}` references `{Module}.Abstractions` and its data project(s)
  - Other modules reference only `{Module}.Abstractions` when cross-module communication is required
  - Shared libraries (Core, IntegrationEvents) are referenced where needed; avoid circular dependencies

## Testing strategy

- Use xUnit for all `{Module}.Tests` projects.
- Aim for high coverage at module boundaries and critical paths.
- Include architecture tests (e.g., analyzers or ArchUnit-like checks) to enforce no cross-module data access and dependency rules.
- For API hosts, include minimal API-level tests and rely on module tests for business rules.

## Example: Orders module (SQL Server)

```
src/
  Orders/
    Company.Product.Orders/
    Company.Product.Orders.Abstractions/
    Company.Product.Orders.Data.SqlServer/
    Company.Product.Orders.Api/
    Company.Product.Orders.Tests/
  Core/
    Company.Product.Core/
    Company.Product.IntegrationEvents/
  Aspire/
    Company.Product.AppHost/
    Company.Product.ServiceDefaults/
```

## Cross-references

- ADR-003: Prefer Modular Monoliths
- ADR-002: Adoption of Aspire for Distributed Application Development
- Style Guide: C# Code Style Guide
