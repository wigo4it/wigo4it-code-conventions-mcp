---
title: C# Code Style Guide
category: StyleGuide
language: CSharp
status: Active
last-updated: 2025-11-13
tags: [csharp, style-guide, coding-conventions, code-quality]
reference: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
---

# C# Code Style Guide

## Overview

This style guide requires all C# developers at Wigo4it to comply with the Microsoft C# coding conventions. These conventions ensure code readability, consistency, and collaboration within development teams. All C# code must follow the guidelines documented by Microsoft at: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

## Goals and Principles

Our coding conventions are based on the following goals:

1. **Correctness**: Code should be resilient and correct, even after multiple edits
2. **Readability**: Code should be easy to understand and maintain
3. **Consistency**: All code should conform to the same style across projects
4. **Modernity**: Aggressively adopt new language features and best practices

## Tools and Enforcement

### Required Tools

Teams **MUST** use the following tools to enforce these conventions:

1. **Code Analysis**: Enable [.NET code analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) in all projects
2. **EditorConfig**: Use an `.editorconfig` file to enforce style guidelines automatically
3. **Analyzers**: Implement Roslyn analyzers to detect rule violations

### EditorConfig

All projects **MUST** include an `.editorconfig` file. You can start with the [dotnet/docs .editorconfig](https://github.com/dotnet/docs/blob/main/.editorconfig) as a baseline and customize as needed.

### CI/CD Integration

Code analysis **MUST** be integrated into CI/CD pipelines to ensure compliance. Builds should fail if code analysis detects violations of configured rules.

## General Guidelines

### Modern C# Features

- **MUST** utilize modern language features and C# versions whenever possible
- **MUST NOT** use outdated language constructs
- **SHOULD** use C# 14 features where appropriate (primary constructors, collection expressions, etc.)

### Data Types

- **MUST** use language keywords for data types instead of runtime types
  - ✅ Use `string` instead of `System.String`
  - ✅ Use `int` instead of `System.Int32`
  - ✅ This applies to `nint` and `nuint` as well
- **SHOULD** use `int` rather than unsigned types for consistency and library interoperability
  - Exception: Documentation or code specific to unsigned data types

### Code Clarity

- **MUST** write code with clarity and simplicity in mind
- **MUST NOT** create overly complex and convoluted code logic
- **SHOULD** favor readability over cleverness

## Language Guidelines

### String Data

#### String Interpolation

**MUST** use string interpolation for concatenating short strings:

```csharp
// ✅ Correct
string displayName = $"{nameList[n].LastName}, {nameList[n].FirstName}";

// ❌ Incorrect
string displayName = nameList[n].LastName + ", " + nameList[n].FirstName;
```

**MUST** use expression-based string interpolation rather than positional string formatting:

```csharp
// ✅ Correct
Console.WriteLine($"{student.Last} Score: {student.score}");

// ❌ Incorrect
Console.WriteLine("{0} Score: {1}", student.Last, student.score);
```

#### StringBuilder

**MUST** use `StringBuilder` when appending strings in loops, especially with large amounts of text:

```csharp
var phrase = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
var manyPhrases = new StringBuilder();
for (var i = 0; i < 10000; i++)
{
    manyPhrases.Append(phrase);
}
```

#### Raw String Literals

**SHOULD** prefer raw string literals to escape sequences or verbatim strings:

```csharp
var message = """
    This is a long message that spans across multiple lines.
    It uses raw string literals. This means we can 
    also include characters like \n and \t without escaping them.
    """;
```

### Constructors and Initialization

#### Primary Constructors

**MUST** use Pascal case for primary constructor parameters on record types:

```csharp
public record Person(string FirstName, string LastName);
```

**MUST** use camel case for primary constructor parameters on class and struct types:

```csharp
public class LabelledContainer<T>(string label)
{
    public string Label { get; } = label;
}
```

#### Required Properties

**SHOULD** use `required` properties instead of constructors to force initialization:

```csharp
public class LabelledContainer<T>(string label)
{
    public string Label { get; } = label;
    public required T Contents 
    { 
        get;
        init;
    }
}
```

### Arrays and Collections

**MUST** use collection expressions to initialize all collection types:

```csharp
// ✅ Correct
string[] vowels = [ "a", "e", "i", "o", "u" ];

// ❌ Incorrect
string[] vowels = new string[] { "a", "e", "i", "o", "u" };
```

### Delegates

#### Func<> and Action<>

**MUST** use `Func<>` and `Action<>` instead of defining delegate types:

```csharp
// ✅ Correct
Action<string> actionExample1 = x => Console.WriteLine($"x is: {x}");
Func<string, int> funcExample1 = x => Convert.ToInt32(x);
Func<int, int, int> funcExample2 = (x, y) => x + y;

// ❌ Incorrect - defining custom delegate types unnecessarily
public delegate void MyAction(string message);
```

#### Delegate Instantiation

**MUST** use concise syntax when creating delegate instances:

```csharp
// ✅ Correct
Del exampleDel = DelMethod;
exampleDel("Hey");

// ❌ Incorrect - verbose syntax
Del exampleDel = new Del(DelMethod);
exampleDel("Hey");
```

### Exception Handling

#### try-catch Statements

**MUST** use try-catch statements for exception handling:

```csharp
static double ComputeDistance(double x1, double y1, double x2, double y2)
{
    try
    {
        return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }
    catch (System.ArithmeticException ex)
    {
        Console.WriteLine($"Arithmetic overflow or underflow: {ex}");
        throw;
    }
}
```

**MUST NOT** catch exceptions that cannot be properly handled:

```csharp
// ❌ Incorrect - catching general exceptions without handling
try
{
    DoSomething();
}
catch (Exception)
{
    // No handling logic
}
```

**MUST** use specific exception types to provide meaningful error messages

#### using Statements

**MUST** simplify resource cleanup using `using` statements:

```csharp
// ✅ Correct - Modern using declaration
using Font normalStyle = new Font("Arial", 10.0f);
byte charset = normalStyle.GdiCharSet;

// ✅ Acceptable - Traditional using statement
using (Font arial = new Font("Arial", 10.0f))
{
    byte charset2 = arial.GdiCharSet;
}

// ❌ Incorrect - Manual Dispose in try-finally
Font bodyStyle = new Font("Arial", 10.0f);
try
{
    byte charset = bodyStyle.GdiCharSet;
}
finally
{
    bodyStyle?.Dispose();
}
```

### Boolean Operators

**MUST** use `&&` instead of `&` and `||` instead of `|` for boolean comparisons to enable short-circuit evaluation:

```csharp
// ✅ Correct - Short-circuits if divisor is 0
if ((divisor != 0) && (dividend / divisor) is var result)
{
    Console.WriteLine($"Quotient: {result}");
}

// ❌ Incorrect - Always evaluates both expressions, potential runtime error
if ((divisor != 0) & (dividend / divisor) is var result)
{
    Console.WriteLine($"Quotient: {result}");
}
```

### Object Instantiation

#### new Operator

**MUST** use concise forms of object instantiation when the variable type matches the object type:

```csharp
// ✅ Correct - Target-typed new
var firstExample = new ExampleClass();
ExampleClass instance2 = new();

// ✅ Acceptable - Explicit type
ExampleClass secondExample = new ExampleClass();
```

**SHOULD** use object initializers to simplify object creation:

```csharp
// ✅ Correct
var thirdExample = new ExampleClass 
{ 
    Name = "Desktop", 
    ID = 37414,
    Location = "Redmond", 
    Age = 2.3 
};

// ❌ Incorrect - Verbose property assignment
var fourthExample = new ExampleClass();
fourthExample.Name = "Desktop";
fourthExample.ID = 37414;
fourthExample.Location = "Redmond";
fourthExample.Age = 2.3;
```

### Event Handling

**SHOULD** use lambda expressions for event handlers that don't need to be removed later:

```csharp
// ✅ Correct
public Form2()
{
    this.Click += (s, e) =>
    {
        MessageBox.Show(((MouseEventArgs)e).Location.ToString());
    };
}

// ❌ Incorrect - Verbose traditional handler
public Form1()
{
    this.Click += new EventHandler(Form1_Click);
}

void Form1_Click(object? sender, EventArgs e)
{
    MessageBox.Show(((MouseEventArgs)e).Location.ToString());
}
```

### Static Members

**MUST** call static members using the class name:

```csharp
// ✅ Correct
ClassName.StaticMember();

// ❌ Incorrect - Using derived class name for base class static member
DerivedClass.StaticMember(); // where StaticMember is defined in base class
```

This practice makes code more readable by making static access clear and prevents potential confusion.

### LINQ Queries

#### Query Variables

**MUST** use meaningful names for query variables:

```csharp
// ✅ Correct
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       select customer.Name;

// ❌ Incorrect
var temp = from c in Customers
           where c.City == "Seattle"
           select c.Name;
```

#### Anonymous Types

**MUST** use aliases to ensure property names are correctly capitalized with Pascal casing:

```csharp
var localDistributors =
    from customer in Customers
    join distributor in Distributors on customer.City equals distributor.City
    select new { Customer = customer, Distributor = distributor };
```

**MUST** rename properties when property names in the result would be ambiguous:

```csharp
var localDistributors2 =
    from customer in Customers
    join distributor in Distributors on customer.City equals distributor.City
    select new { CustomerName = customer.Name, DistributorName = distributor.Name };
```

#### Implicit Typing

**MUST** use implicit typing in LINQ query declarations:

```csharp
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       select customer.Name;
```

#### Query Formatting

**MUST** align query clauses under the `from` clause:

```csharp
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       orderby customer.Name
                       select customer;
```

**MUST** use `where` clauses before other query clauses to filter data early:

```csharp
var seattleCustomers2 = from customer in Customers
                        where customer.City == "Seattle"
                        orderby customer.Name
                        select customer;
```

#### Multiple from Clauses

**SHOULD** use multiple `from` clauses instead of `join` to access inner collections:

```csharp
var scoreQuery = from student in students
                 from score in student.Scores
                 where score > 90
                 select new { Last = student.LastName, score };
```

### Implicitly Typed Local Variables

#### When to Use var

**MUST** use implicit typing (`var`) when the type is obvious from the right side of the assignment:

```csharp
// ✅ Correct - Type is obvious
var message = "This is clearly a string.";
var currentTemperature = 27;

// ❌ Incorrect - Type is obvious but not using var
string message = "This is clearly a string.";
int currentTemperature = 27;
```

**MUST NOT** use `var` when the type isn't apparent from the right side:

```csharp
// ✅ Correct - Type is not obvious
int numberOfIterations = Convert.ToInt32(Console.ReadLine());
int currentMaximum = ExampleClass.ResultSoFar();

// ❌ Incorrect - Type is not obvious
var numberOfIterations = Convert.ToInt32(Console.ReadLine());
var currentMaximum = ExampleClass.ResultSoFar();
```

**MUST NOT** use variable names to indicate type:

```csharp
// ❌ Incorrect - Variable name indicates type
var inputInt = Console.ReadLine();

// ✅ Correct - Type specifies type, variable name specifies meaning
int iterations = Convert.ToInt32(Console.ReadLine());
```

#### var vs dynamic

**MUST NOT** use `var` in place of `dynamic`:

```csharp
// ✅ Correct - Use dynamic when you need run-time type inference
dynamic jsonObject = GetDynamicObject();

// ❌ Incorrect - Using var when dynamic behavior is needed
var jsonObject = GetDynamicObject(); // May not provide expected dynamic behavior
```

#### Loop Variables

**MUST** use implicit typing for loop variables in `for` loops:

```csharp
// ✅ Correct
for (var i = 0; i < 10000; i++)
{
    manyPhrases.Append(phrase);
}

// ❌ Incorrect
for (int i = 0; i < 10000; i++)
{
    manyPhrases.Append(phrase);
}
```

**MUST NOT** use implicit typing for loop variables in `foreach` loops:

```csharp
// ✅ Correct - Explicit type makes it clear what we're iterating
foreach (char ch in laugh)
{
    Console.Write(ch);
}

// ❌ Incorrect - Collection name alone doesn't indicate element type
foreach (var ch in laugh)
{
    Console.Write(ch);
}
```

**Exception**: MUST use implicit typing for LINQ query results (often anonymous types):

```csharp
var queryResult = from customer in Customers
                  where customer.City == "Seattle"
                  select new { customer.Name, customer.City };
```

### Namespace Declarations

#### File-Scoped Namespaces

**MUST** use file-scoped namespace declarations:

```csharp
// ✅ Correct
namespace MySampleCode;

public class MyClass
{
    // Implementation
}

// ❌ Incorrect
namespace MySampleCode
{
    public class MyClass
    {
        // Implementation
    }
}
```

#### using Directives

**MUST** place `using` directives outside the namespace declaration:

```csharp
// ✅ Correct - using directive outside namespace
using Azure;

namespace CoolStuff.AwesomeFeature;

public class Awesome
{
    public void Stuff()
    {
        WaitUntil wait = WaitUntil.Completed;
    }
}

// ❌ Incorrect - using directive inside namespace (context-sensitive)
namespace CoolStuff.AwesomeFeature
{
    using Azure;

    public class Awesome
    {
        public void Stuff()
        {
            WaitUntil wait = WaitUntil.Completed;
        }
    }
}
```

**Rationale**: `using` directives inside namespaces are context-sensitive and can cause ambiguous name resolution when new namespaces are added to dependencies.

## Style Guidelines

### Comment Style

#### Comment Format

**MUST** use single-line comments (`//`) for brief explanations:

```csharp
// The following declaration creates a query. It does not run the query.
var query = from item in collection where item.IsActive select item;
```

**MUST NOT** use multi-line comments (`/* */`) for longer explanations

**MUST** use XML comments for documenting public APIs:

```csharp
/// <summary>
/// Calculates the distance between two points.
/// </summary>
/// <param name="x1">The x-coordinate of the first point.</param>
/// <param name="y1">The y-coordinate of the first point.</param>
/// <param name="x2">The x-coordinate of the second point.</param>
/// <param name="y2">The y-coordinate of the second point.</param>
/// <returns>The distance between the two points.</returns>
public static double ComputeDistance(double x1, double y1, double x2, double y2)
{
    return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
}
```

#### Comment Placement

**MUST** place comments on a separate line, not at the end of a line of code

**MUST** begin comment text with an uppercase letter

**MUST** end comment text with a period

**MUST** insert one space between the comment delimiter (`//`) and the comment text:

```csharp
// This is a properly formatted comment.
```

### Layout Conventions

#### Indentation and Formatting

**MUST** use default Code Editor settings:
- Smart indenting enabled
- Four-character indents
- Tabs saved as spaces

**MUST** write only one statement per line:

```csharp
// ✅ Correct
var x = 10;
var y = 20;

// ❌ Incorrect
var x = 10; var y = 20;
```

**MUST** write only one declaration per line:

```csharp
// ✅ Correct
int width;
int height;

// ❌ Incorrect
int width, height;
```

**MUST** indent continuation lines one tab stop (four spaces) if not indented automatically

**MUST** add at least one blank line between method definitions and property definitions:

```csharp
public class Example
{
    public int Property1 { get; set; }

    public int Property2 { get; set; }

    public void Method1()
    {
        // Implementation
    }

    public void Method2()
    {
        // Implementation
    }
}
```

#### Parentheses and Expressions

**SHOULD** use parentheses to make clauses in an expression apparent:

```csharp
// ✅ Correct - Parentheses make the logic clear
if ((startX > endX) && (startX > previousX))
{
    // Take appropriate action.
}

// ❌ Incorrect - Harder to read without grouping
if (startX > endX && startX > previousX)
{
    // Take appropriate action.
}
```

**Exception**: When demonstrating operator or expression precedence

#### Code Formatting Standards

**MUST** use four spaces for indentation (no tabs)

**MUST** align code consistently to improve readability

**SHOULD** limit lines to 65 characters to enhance readability (especially for documentation)

**MUST** break long statements into multiple lines for clarity

**MUST** use "Allman" style for braces:
- Opening and closing braces on their own lines
- Braces line up with current indentation level

```csharp
// ✅ Correct - Allman style
if (condition)
{
    DoSomething();
}

// ❌ Incorrect - K&R style
if (condition) {
    DoSomething();
}
```

**MUST** place line breaks before binary operators when necessary:

```csharp
// ✅ Correct
var result = longVariableName
    + anotherLongVariableName
    + yetAnotherLongVariableName;

// ❌ Incorrect
var result = longVariableName +
    anotherLongVariableName +
    yetAnotherLongVariableName;
```

## Asynchronous Programming

**MUST** use asynchronous programming with `async` and `await` for I/O-bound operations:

```csharp
public async Task<string> FetchDataAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}
```

**MUST** be cautious of deadlocks and use `Task.ConfigureAwait` when appropriate in library code:

```csharp
// In library code
public async Task<Data> GetDataAsync()
{
    var result = await FetchFromDatabaseAsync().ConfigureAwait(false);
    return result;
}
```

## Security

**MUST** follow the guidelines in [Secure Coding Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)

**MUST** validate all external input

**MUST** use parameterized queries to prevent SQL injection

**MUST** properly dispose of sensitive data

**MUST** use secure communication protocols (HTTPS, TLS)

## Compliance and Enforcement

### Required Compliance

All C# code developed or modified at Wigo4it **MUST** comply with these conventions. Code reviews **MUST** verify adherence to these guidelines.

### Code Reviews

Code reviewers **MUST**:
1. Verify compliance with these conventions
2. Ensure proper use of EditorConfig and analyzers
3. Check that code analysis is enabled and passing
4. Reject pull requests that violate these standards

### Continuous Improvement

These guidelines **SHOULD** be updated as:
- New C# language features are released
- Microsoft updates their conventions
- Team identifies areas for improvement

### References

- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Runtime C# Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Secure Coding Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)
- [dotnet/docs .editorconfig](https://github.com/dotnet/docs/blob/main/.editorconfig)

---

**Last Updated**: November 13, 2025  
**Version**: 1.0  
**Status**: Active - All teams must comply
