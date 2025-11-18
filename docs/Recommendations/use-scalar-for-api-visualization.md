---
title: Use Scalar for Local API Exploration and Debugging
category: Recommendations
status: Active
last-updated: 2025-11-18
applicability: All API projects
tags: [scalar, openapi, api-visualization, debugging, developer-experience, aspire, openrouter]
related:
  - StyleGuides/csharp-style-guide.md
  - Structures/dotnet-project-structure.md
---

# Recommendation: Use Scalar for Local API Exploration

Scalar is a lightweight tool for visualizing REST APIs by leveraging their OpenAPI (Swagger) descriptions. When used as part of your local development workflow, Scalar helps engineers inspect API endpoints, input/output schemas, and example requests — right in the browser — without extra manual setup. This makes debugging API behavior and testing endpoint interactions far easier.

## Why Scalar?

- Rapidly explore contract-level details (paths, parameters, response types) from a single UI.
- Provides an explorer view that complements existing tools such as Swagger UI or Postman.
- Works well with automated documentation pipelines: a single OpenAPI JSON can drive multiple explorers without extra authoring.
- Can be configured to run only during development and debugging (never served in production), improving security and local developer ergonomics.

## Recommended Setup

1. Generate and expose an OpenAPI (Swagger) JSON from your API. Using `Swashbuckle.AspNetCore` or similar is common in .NET projects; the schema endpoint is usually at `/swagger/v1/swagger.json`.

2. Add Scalar to your developer environment. You can host Scalar in a small static site or add a reserved route in the API that serves the Scalar UI.

3. Ensure Scalar is only enabled during development and debugging:
   - Add gating logic in your `Program.cs` or startup to mount the Scalar UI only when `app.Environment.IsDevelopment()` is true.

Example (minimal hosting):

```csharp
// Program.cs (minimal hosting model) - .NET 10 / C# 14
if (app.Environment.IsDevelopment())
{
    // Serve static files from wwwroot/scalar - include the Scalar distribution in your project or host it separately
    app.UseStaticFiles();

    // Optional: map the Scalar UI to a friendly path
    app.MapFallbackToFile("/scalar/{*path}", "scalar/index.html");
}
```

4. Make Scalar the launch URL for your API while debugging. In `Properties/launchSettings.json`, add a `launchUrl` to the debug profile that opens the scalar endpoint and points it at your OpenAPI document.

Example `launchSettings.json` snippet:

```json
{
  "profiles": {
    "MyApi": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "scalar/index.html?openapi=/swagger/v1/swagger.json",
      "applicationUrl": "https://localhost:7001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

This configuration opens Scalar in your browser when you start debugging — the `openapi` query parameter instructs Scalar to fetch the OpenAPI document from the API endpoint (the `openapi` parameter name may vary by chosen Scalar distribution; consult the Scalar docs if you host a custom build).

## Scalar + Aspire

Aspire integrates tightly with Scalar — you can add a Scalar API reference directly from your Aspire `AppHost`, which provides a better integrated developer experience and avoids common CORS problems by using a built-in proxy.

Key points from the Aspire+Scalar integration:

- Add the `Scalar.Aspire` package to your `AppHost` and configure the integration with `AddScalarApiReference`:

```csharp
// AppHost / Program.cs
var scalar = builder.AddScalarApiReference(options =>
{
  // Optional theme used by Scalar UX
  options.WithTheme(ScalarTheme.Laserwave);
});

// Provide a service reference to API endpoints
scalar.WithApiReference(apiService);
```

- The Aspire Scalar integration registers a proxy path (commonly `/scalar-proxy`), backed by YARP (Yet Another Reverse Proxy) so the UI can call your API without CORS issues. This proxy is enabled by default in the Scalar Aspire integration and is very helpful during local debugging.
- Scalar + Aspire can be configured to run only in development. Use `app.Environment.IsDevelopment()` gating or a configuration flag (e.g., `Documentation:EnableScalar`) to ensure Scalar and the proxy are not exposed in production.

Practical tips:
- The proxy makes it easy to test API endpoints that require special headers or AI keys since the proxy can forward authentication headers (be mindful of how secrets are handled).
- Aspire + Scalar examples often pair well with AI features such as Structured Output and OpenRouter; Scalar is primarily a UI for API exploration while Aspire provides the API/AI runtime.

Read more:
- Aspire & Scalar integration: https://github.com/scalar/scalar/tree/main/integrations/aspire
- Scalar docs: https://guides.scalar.com/scalar/scalar-api-references/integrations/net-aspire
- Tutorial (aspire + scalar + openrouter): https://dev.to/victorioberra/build-an-aspire-api-using-microsoft-openai-scalar-openrouter-structured-output-and-custom-3a6

### Take Aspire to the mic

If you use Aspire to host AI-backed endpoints, opening Scalar as the `launchUrl` makes your API demos and debugging sessions highly interactive. Start the API in debug mode and Scalar will load your OpenAPI schema; you can then call endpoints such as an AI-generated `weatherforecast` to inspect structured output and test variations — great when teaching others or demonstrating new AI features.

## Tips & Best Practices

- Ignore Scalar in production: ensure the route to the UI is gated by `IsDevelopment()` or a specific configuration flag such as `Documentation:EnableScalar` set to `false` in production.
- Reuse your canonical OpenAPI JSON: point Scalar at `/swagger/v1/swagger.json`; this keeps documentation consistent between UI tools and avoids duplication.
- Pair with `launchUrl` so the browser opens the explorer and the `openapi` parameter populates the API contract automatically. This makes it very easy for developers to start tests and sanity checks on endpoints while debugging.
- Consider adding a small overlay or README section for first-time contributors describing where to find the Scalar UI and how to use it safely.

## Example flow for a developer

1. Clone the repository and run the server in debug mode (F5).
2. The browser opens `https://localhost:7001/scalar/index.html?openapi=/swagger/v1/swagger.json`.
3. Use Scalar to search endpoints, explore request/response schemas, and run ad-hoc test requests.
4. Fix, retest, repeat.

## Security & Access

- Scalar should be considered a developer convenience and not a security boundary. Do not expose Scalar in non-development environments (staging/production) unless you have additional access controls in place.
- If you need a shared explorer in staging, prefer a gated, authenticated installer of a similar UI, or provide OpenAPI access only to authorized users.

## Conclusion

Scalar is a practical, low-cost addition to any API-first workflow. Enabling it just for debugging provides immediate developer benefits — quick contract discovery and ad-hoc testing — without adding production risk. Combine it with a sensible `launchUrl` configuration to make API testing during debugging a breeze.
