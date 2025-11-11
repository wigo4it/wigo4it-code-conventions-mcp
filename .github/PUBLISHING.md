# Publishing Workflow

This document describes the automated NuGet package publishing workflow for the Wigo4it Coding Guidelines MCP Server.

## Overview

The project uses **GitVersion** with **GitHub Flow** to automatically version and publish NuGet packages to the **GitHub Packages** feed whenever code is pushed to the `main` branch.

## How It Works

### 1. GitVersion Configuration

The project uses GitVersion configured with **GitHub Flow** (`workflow: GitHubFlow/v1`) and **ContinuousDeployment** mode.

**Configuration file**: `GitVersion.yml`

```yaml
workflow: GitHubFlow/v1
mode: ContinuousDeployment
branches:
  main:
    mode: ContinuousDeployment
    increment: Patch
```

### 2. Semantic Versioning

GitVersion automatically calculates the next version based on:

- **Git tags**: Previous releases (e.g., `v1.0.0`, `v1.2.3`)
- **Commit messages**: Special markers to control versioning
- **Branch name**: Determines pre-release labels

#### Version Bump via Commit Messages

You can control version increments using commit message markers:

| Commit Message Marker | Version Impact | Example |
|----------------------|----------------|---------|
| `+semver: major` or `+semver: breaking` | Major version bump (1.0.0 → 2.0.0) | `feat: redesign API +semver: major` |
| `+semver: minor` or `+semver: feature` | Minor version bump (1.0.0 → 1.1.0) | `feat: add new guideline +semver: minor` |
| `+semver: patch` or `+semver: fix` | Patch version bump (1.0.0 → 1.0.1) | `fix: correct typo +semver: patch` |
| `+semver: none` or `+semver: skip` | No version bump | `docs: update README +semver: none` |

**Default behavior**: If no marker is specified, GitVersion will increment the **patch** version on every commit to `main`.

### 3. GitHub Actions Workflow

**File**: `.github/workflows/publish-nuget.yml`

The workflow automatically runs when:
- ✅ Code is pushed to the `main` branch
- ✅ Changes are made to `src/**`, `GitVersion.yml`, or the workflow file itself
- ❌ **Does NOT run** on pull requests

#### Workflow Steps

1. **Checkout repository** with full history (`fetch-depth: 0`)
2. **Setup .NET 9.0**
3. **Install GitVersion**
4. **Calculate version** using GitVersion
5. **Restore dependencies**
6. **Build solution** with version metadata
7. **Run tests** to ensure quality
8. **Pack NuGet packages**:
   - `Wigo4it.CodingGuidelines.Core`
   - `Wigo4it.CodingGuidelines.McpServer`
9. **Publish to GitHub Packages** using `GITHUB_TOKEN`
10. **Create Git tag** (e.g., `v1.2.3`)

### 4. Generated Packages

Two NuGet packages are published:

#### Wigo4it.CodingGuidelines.Core
- **Description**: Core library for loading and managing guidelines
- **Usage**: Reference this library if you want to build custom tooling

#### Wigo4it.CodingGuidelines.McpServer
- **Description**: MCP server executable
- **Usage**: Install as a .NET tool to run the MCP server
- **Command**: `wigo4it-guidelines-mcp`

## Using the Published Packages

### Configure GitHub Packages as a Source

Add GitHub Packages to your NuGet sources:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/wigo4it/index.json" \
  --name "Wigo4it GitHub Packages" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

**Note**: You need a GitHub Personal Access Token (PAT) with `read:packages` permission.

### Install the MCP Server as a .NET Tool

```bash
dotnet tool install --global Wigo4it.CodingGuidelines.McpServer \
  --add-source "https://nuget.pkg.github.com/wigo4it/index.json"
```

Then run it:

```bash
wigo4it-guidelines-mcp
```

### Reference the Core Library

In your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Wigo4it.CodingGuidelines.Core" Version="1.0.0" />
</ItemGroup>
```

## Version Strategy Examples

### Example 1: Patch Release (Default)

```bash
git commit -m "fix: correct typo in ADR 0002"
git push origin main
```

**Result**: `1.0.0` → `1.0.1`

### Example 2: Minor Release (New Feature)

```bash
git commit -m "feat: add TypeScript naming guidelines +semver: minor"
git push origin main
```

**Result**: `1.0.1` → `1.1.0`

### Example 3: Major Release (Breaking Change)

```bash
git commit -m "feat: redesign document structure +semver: major"
git push origin main
```

**Result**: `1.1.0` → `2.0.0`

### Example 4: Documentation Update (No Version Bump)

```bash
git commit -m "docs: improve README examples +semver: none"
git push origin main
```

**Result**: `2.0.0` → `2.0.0` (no change, but package is republished)

## Tagging Strategy

GitVersion automatically creates and pushes Git tags:

- Each successful build on `main` creates a tag like `v1.2.3`
- Tags mark official releases in the GitHub repository
- Tags are used by GitVersion to calculate subsequent versions

### Manual Tagging (Alternative)

You can also manually create tags to set specific versions:

```bash
git tag v1.0.0
git push origin v1.0.0
```

GitVersion will use this as the base version for future calculations.

## Troubleshooting

### Workflow Not Running

**Check**:
1. Verify you pushed to `main` (not a feature branch)
2. Ensure changes are in `src/**` or workflow-related files
3. Check GitHub Actions tab for error messages

### Version Not Incrementing

**Check**:
1. Ensure `fetch-depth: 0` is set in workflow (full Git history required)
2. Verify GitVersion.yml is valid
3. Check if `+semver: none` was used in commit message
4. Review GitVersion output in workflow logs

### Package Already Exists

GitVersion ensures unique versions, but if republishing is needed:

1. Delete the package version from GitHub Packages
2. Or increment the version manually with a tag

### Authentication Failures

**Check**:
1. `GITHUB_TOKEN` has `packages: write` permission (automatic in workflows)
2. For local installations, ensure your PAT has `read:packages` scope

## Continuous Deployment Philosophy

This workflow implements **Continuous Deployment** on `main`:

- ✅ Every commit to `main` is a potential release
- ✅ Versions increment automatically
- ✅ Packages are published immediately
- ✅ Quality gates (tests) prevent bad releases

**Best Practice**: Use feature branches and pull requests for development. Only merge to `main` when ready to release.

## Monitoring Releases

### GitHub Actions Tab
View workflow runs at: `https://github.com/wigo4it/wigo4it-code-conventions-mcp/actions`

### GitHub Packages Tab
View published packages at: `https://github.com/orgs/wigo4it/packages`

### Git Tags
List all releases:
```bash
git tag -l
```

## Resources

- [GitVersion Documentation](https://gitversion.net/)
- [GitHub Flow Workflow](https://gitversion.net/docs/reference/modes/continuous-deployment)
- [GitHub Packages Documentation](https://docs.github.com/en/packages)
- [Semantic Versioning](https://semver.org/)
