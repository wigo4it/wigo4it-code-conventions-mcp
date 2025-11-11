using Wigo4it.CodingGuidelines.Core.Models;

namespace Wigo4it.CodingGuidelines.Core.Services;

/// <summary>
/// Service for managing coding guidelines, style guides, and ADRs
/// </summary>
public class GuidelinesService
{
    private readonly List<CodingGuideline> _codingGuidelines;
    private readonly List<StyleGuide> _styleGuides;
    private readonly List<ArchitectureDecisionRecord> _adrs;

    public GuidelinesService()
    {
        _codingGuidelines = InitializeCodingGuidelines();
        _styleGuides = InitializeStyleGuides();
        _adrs = InitializeADRs();
    }

    public List<CodingGuideline> GetAllCodingGuidelines()
    {
        return _codingGuidelines;
    }

    public CodingGuideline? GetCodingGuidelineById(string id)
    {
        return _codingGuidelines.FirstOrDefault(g => g.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public List<CodingGuideline> GetCodingGuidelinesByCategory(string category)
    {
        return _codingGuidelines
            .Where(g => g.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<CodingGuideline> GetCodingGuidelinesByLanguage(string language)
    {
        return _codingGuidelines
            .Where(g => g.Language != null && g.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<StyleGuide> GetAllStyleGuides()
    {
        return _styleGuides;
    }

    public StyleGuide? GetStyleGuideById(string id)
    {
        return _styleGuides.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public StyleGuide? GetStyleGuideByLanguage(string language)
    {
        return _styleGuides.FirstOrDefault(s => s.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
    }

    public List<ArchitectureDecisionRecord> GetAllADRs()
    {
        return _adrs;
    }

    public ArchitectureDecisionRecord? GetADRById(string id)
    {
        return _adrs.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public List<ArchitectureDecisionRecord> GetADRsByStatus(string status)
    {
        return _adrs
            .Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static List<CodingGuideline> InitializeCodingGuidelines()
    {
        return new List<CodingGuideline>
        {
            new CodingGuideline
            {
                Id = "CG001",
                Title = "Use Meaningful Variable Names",
                Description = "Variables should have descriptive names that clearly indicate their purpose. Avoid single-letter names except for loop counters.",
                Category = "Naming",
                Example = "// Good\nstring customerName = \"John Doe\";\n\n// Bad\nstring cn = \"John Doe\";",
                Tags = new List<string> { "naming", "readability" },
                Language = "C#"
            },
            new CodingGuideline
            {
                Id = "CG002",
                Title = "PascalCase for Class Names",
                Description = "All class names should use PascalCase convention where each word starts with a capital letter.",
                Category = "Naming",
                Example = "// Good\npublic class CustomerService { }\n\n// Bad\npublic class customerservice { }",
                Tags = new List<string> { "naming", "convention" },
                Language = "C#"
            },
            new CodingGuideline
            {
                Id = "CG003",
                Title = "Single Responsibility Principle",
                Description = "A class should have only one reason to change. Each class should have a single, well-defined responsibility.",
                Category = "Design",
                Example = "// Good: Separate concerns\npublic class CustomerRepository { }\npublic class EmailService { }\n\n// Bad: Mixed concerns\npublic class CustomerManager { /* handles data AND email */ }",
                Tags = new List<string> { "solid", "design", "architecture" },
                Language = null
            },
            new CodingGuideline
            {
                Id = "CG004",
                Title = "Async Methods Should End with Async",
                Description = "All asynchronous methods should have 'Async' suffix to clearly indicate their nature.",
                Category = "Naming",
                Example = "// Good\npublic async Task<Customer> GetCustomerAsync(int id)\n\n// Bad\npublic async Task<Customer> GetCustomer(int id)",
                Tags = new List<string> { "naming", "async", "convention" },
                Language = "C#"
            }
        };
    }

    private static List<StyleGuide> InitializeStyleGuides()
    {
        return new List<StyleGuide>
        {
            new StyleGuide
            {
                Id = "SG001",
                Title = "C# Coding Style",
                Language = "C#",
                Content = @"# C# Coding Style Guide

## Naming Conventions
- Classes: PascalCase
- Methods: PascalCase
- Properties: PascalCase
- Private fields: _camelCase with underscore prefix
- Local variables: camelCase
- Constants: PascalCase

## Formatting
- Use 4 spaces for indentation (not tabs)
- Opening braces on new line
- One statement per line
- Use var when type is obvious

## Best Practices
- Prefer async/await over Task.Result
- Use using statements for IDisposable
- Avoid nested ternary operators
- Keep methods short and focused",
                Tags = new List<string> { "csharp", "formatting", "naming" }
            },
            new StyleGuide
            {
                Id = "SG002",
                Title = "TypeScript Coding Style",
                Language = "TypeScript",
                Content = @"# TypeScript Coding Style Guide

## Naming Conventions
- Classes: PascalCase
- Interfaces: PascalCase (without 'I' prefix)
- Methods: camelCase
- Properties: camelCase
- Constants: UPPER_SNAKE_CASE
- Type parameters: Single capital letter or PascalCase

## Formatting
- Use 2 spaces for indentation
- Use semicolons
- Prefer single quotes for strings
- Use trailing commas in multiline

## Best Practices
- Always specify types explicitly
- Use interfaces for object shapes
- Prefer readonly for immutable properties
- Use strict mode",
                Tags = new List<string> { "typescript", "javascript", "formatting" }
            }
        };
    }

    private static List<ArchitectureDecisionRecord> InitializeADRs()
    {
        return new List<ArchitectureDecisionRecord>
        {
            new ArchitectureDecisionRecord
            {
                Id = "ADR001",
                Title = "Use Model Context Protocol for AI Integration",
                Context = "We need a standardized way to provide coding guidelines and architectural decisions to AI assistants and developers.",
                Decision = "Implement an MCP server using .NET 9 to expose our coding guidelines, style guides, and ADRs through a structured API.",
                Consequences = "Positive: Standardized access to guidelines, better AI integration, extensible architecture. Negative: Additional infrastructure to maintain.",
                Status = "Accepted",
                DateCreated = DateTime.Parse("2025-01-15"),
                Tags = new List<string> { "architecture", "ai", "integration" }
            },
            new ArchitectureDecisionRecord
            {
                Id = "ADR002",
                Title = "Adopt Microservices Architecture",
                Context = "Our monolithic application has become difficult to maintain and scale. Different teams work on different features and deployments are risky.",
                Decision = "Break down the monolith into microservices, each responsible for a specific business capability.",
                Consequences = "Positive: Better scalability, independent deployments, team autonomy. Negative: Increased operational complexity, distributed system challenges.",
                Status = "Accepted",
                DateCreated = DateTime.Parse("2024-06-01"),
                Tags = new List<string> { "architecture", "microservices", "scalability" }
            },
            new ArchitectureDecisionRecord
            {
                Id = "ADR003",
                Title = "Use Repository Pattern for Data Access",
                Context = "Direct database access throughout the application makes it difficult to test and change data sources.",
                Decision = "Implement the Repository pattern to abstract data access logic and provide a clean separation between business logic and data access.",
                Consequences = "Positive: Testability, maintainability, flexibility. Negative: Additional abstraction layer, slight performance overhead.",
                Status = "Accepted",
                DateCreated = DateTime.Parse("2024-03-15"),
                Tags = new List<string> { "architecture", "data-access", "design-patterns" }
            }
        };
    }
}
