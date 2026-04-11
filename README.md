# ReactiveUI MCP Server

<!-- mcp-name: io.github.chrispulman/reactiveui-mcp-server -->

ReactiveUI MCP Server gives AI assistants an opinionated, ecosystem-aware source of truth for generating ReactiveUI-family code.

It is designed to help agents and developers choose the correct ReactiveUI packages, startup patterns, binding strategies, source generators, validation approach, caching/networking companions, reactive collection patterns, project-generation guidance, wizard-driven solution planning, on-disk solution generation, and legacy migration strategies across:

- ReactiveUI core
- WPF
- WinForms
- Blazor
- MAUI
- WinUI
- AndroidX
- Avalonia
- Uno
- Splat
- ReactiveUI.Extensions
- Refit
- Akavache
- ReactiveUI.SourceGenerators
- ReactiveUI.Binding.SourceGenerators
- ReactiveUI.Validation
- Fusillade
- punchclock
- DynamicData
- ReactiveUI.Testing

The server is implemented in C# on `net10.0` using:
- `ModelContextProtocol` `1.2.0`

## Quick Install

Click to install in your preferred environment:

[![VS Code - Install ReactiveUI MCP](https://img.shields.io/badge/VS_Code-Install_ReactiveUI_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=reactiveui-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.ReactiveUI.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)
[![VS Code Insiders - Install ReactiveUI MCP](https://img.shields.io/badge/VS_Code_Insiders-Install_ReactiveUI_MCP-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=reactiveui-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.ReactiveUI.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D&quality=insiders)
[![Visual Studio - Install ReactiveUI MCP](https://img.shields.io/badge/Visual_Studio-Install_ReactiveUI_MCP-5C2D91?style=flat-square&logo=visualstudio&logoColor=white)](https://vs-open.link/mcp-install?%7B%22name%22%3A%22CP.ReactiveUI.Mcp.Server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.ReactiveUI.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)

Note:
- These install links are prepared for the intended NuGet package identity `CP.ReactiveUI.Mcp.Server`.
- If the latest package has not been published yet, use the manual source-build configuration below.

## What this MCP server helps with

This MCP server is intended to improve AI-generated C# and XAML/UI code by making the current ReactiveUI ecosystem guidance discoverable through MCP tools, resources, and prompts.

It helps with:
- choosing the correct ReactiveUI package for the platform
- preferring modern builder-based startup where appropriate
- using `WhenActivated` and proper disposal patterns
- preferring `ReactiveCommand`, `WhenAnyValue`, and derived observable state
- preferring `ReactiveUI.SourceGenerators` as the primary code-generation approach for new ReactiveUI code
- generating migration plans away from legacy `ReactiveUI.Fody`
- upgrading legacy ReactiveUI applications to the latest appropriate package combinations and patterns
- recommending `ReactiveUI.Binding.SourceGenerators` for compile-time bindings and AOT/trimming-sensitive scenarios
- recommending `ReactiveUI.Validation` instead of hand-rolled validation glue
- recommending `ReactiveUI.Extensions.Async` and `IObservableAsync`-based approaches where async observable semantics are the right fit
- recommending `Akavache`, `Refit`, `DynamicData`, `Fusillade`, `punchclock`, and `ReactiveUI.Extensions` only when they fit the scenario
- creating new test project guidance using `ReactiveUI.Testing`
- migrating older test projects to current `ReactiveUI.Testing` practices
- guiding users through a wizard-based multi-endpoint solution design flow
- generating a starter solution on disk when requested
- reviewing plans for anti-patterns like `ReactiveUI.ReactiveList`, constructor subscriptions, and global service-location abuse

## Available MCP Tools

The current server surface includes these MCP tools:

- `reactiveui_catalog_list`
  - Lists all known ecosystem areas
- `reactiveui_catalog_search`
  - Searches the catalog by platform, package, or keyword
- `reactiveui_catalog_get`
  - Returns the full manifest for a specific ecosystem area
- `reactiveui_recommend`
  - Recommends packages and patterns for a given platform and feature set
- `reactiveui_review_plan`
  - Reviews a proposed implementation plan for likely ReactiveUI anti-patterns
- `reactiveui_compare`
  - Compares two ecosystem areas side-by-side
- `reactiveui_scaffold_prompt`
  - Produces a high-quality prompt for another AI coding agent
- `reactiveui_project_blueprint`
  - Produces a project-generation blueprint for new ReactiveUI applications, libraries, and test projects
- `reactiveui_migration_plan`
  - Produces a migration plan for legacy ReactiveUI, ReactiveUI.Fody, and ReactiveUI.Testing-based projects
- `reactiveui_create_solution`
  - Runs a wizard-like multi-step solution planning tool for building a ReactiveUI solution with multiple UI endpoints, Splat DI provider selection, optional features, settings stores, themes, validation, scaffolded views/viewmodels, and optional on-disk generation

## Available MCP Resources

- `reactiveui://catalog`
- `reactiveui://ecosystem/{id}`
- `reactiveui://best-practices/project-generation`

Examples:
- `reactiveui://ecosystem/reactiveui-core`
- `reactiveui://ecosystem/reactiveui-maui`
- `reactiveui://ecosystem/akavache`
- `reactiveui://ecosystem/dynamicdata`
- `reactiveui://ecosystem/reactiveui-testing`

## Available MCP Prompts

- `create_reactiveui_scaffold`
- `create_reactiveui_test_project`
- `migrate_legacy_reactiveui_project`

## Example questions/prompts for your AI assistant

Once configured, you can ask things like:

- "Run reactiveui_create_solution and help me design a multi-endpoint solution with WPF, MAUI, and Blazor"
- "Generate the solution on disk under D:\\Temp\\GeneratedApps using the completed wizard selections"
- "Create a new ReactiveUI MAUI project blueprint with Akavache, Refit, and ReactiveUI.SourceGenerators"
- "Generate a test project blueprint using the latest ReactiveUI.Testing"
- "Migrate this ReactiveUI.Fody WPF app to ReactiveUI.SourceGenerators"
- "Migrate this old test project to the latest ReactiveUI.Testing practices"
- "When should I use ReactiveUI.Extensions.Async and IObservableAsync in this app?"
- "Compare ReactiveUI MAUI and ReactiveUI AndroidX for a new mobile app"
- "Review this ReactiveUI plan for bad patterns"
- "Give me a prompt to generate a ReactiveUI WinUI screen using source generators and validation"
- "What is the recommended DynamicData pattern for sorted live collections in ReactiveUI?"
- "Should I use Akavache or a custom SQLite repository here?"
- "What should a ReactiveUI WPF view do with WhenActivated and DisposeWith?"

## reactiveui_create_solution wizard flow

The wizard-oriented tool is designed to behave like a guided planning assistant for solution creation.

It supports:
- selecting one or more UI endpoints in the same solution
- selecting the Splat-based DI provider package to use
- selecting additional ReactiveUI or ReactiveMarbles family features
- selecting a settings store such as Akavache SQLite or another provider
- selecting common application features like:
  - Authentication
  - Settings Page
  - Theming
  - Navigation
  - Validation
  - Offline Sync
- specifying prime/theme colors
- specifying a validation strategy
- specifying the views required for each endpoint
- generating scaffold guidance for views and view models per endpoint
- including a test project with `ReactiveUI.Testing`
- optionally generating the starter solution on disk

Wizard stages:
- `1` / `start`
  - solution name + UI endpoints
- `2` / `di`
  - Splat DI provider choice
- `3` / `features`
  - extra features beyond direct dependencies like Splat and DynamicData
- `4` / `storage`
  - settings store selection
- `5` / `application`
  - app features, colors, validation
- `6` / `views`
  - endpoint-specific view list
- `7` / `blueprint`
  - review high-level structure
- `8` / `migration`
  - optional migration considerations
- `9` / `complete`
  - final blueprint output, with optional file generation

The tool is called as:
- `reactiveui_create_solution`

To generate files on disk, provide:
- `step = 9` or `complete`
- `generateFiles = true`
- `outputRoot = <target folder>`

## Installation

### Requirements

- .NET 10 SDK
- An MCP-capable client such as VS Code, Visual Studio, or Claude Desktop

### Option 1: Quick Install

Use one of the install badges above.

This is the preferred future installation model once the package is published under:
- `CP.ReactiveUI.Mcp.Server`

### Option 2: Manual configuration from source

If the NuGet package is not published yet, configure the MCP client to run the server from source.

Project path:
- `ReactiveUIMcp.Server\src\ReactiveUIMcp.Server\ReactiveUIMcp.Server.csproj`

### VS Code

Add to settings JSON:

```json
{
  "github.copilot.chat.mcp.servers": {
    "reactiveui-mcp-server": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/ReactiveUI-mcp/ReactiveUIMcp.Server/src/ReactiveUIMcp.Server/ReactiveUIMcp.Server.csproj"
      ]
    }
  }
}
```

### Visual Studio

Add an MCP server with:
- Name: `reactiveui-mcp-server`
- Type: `stdio`
- Command: `dotnet`
- Arguments:

```text
run --project /path/to/ReactiveUI-mcp/ReactiveUIMcp.Server/src/ReactiveUIMcp.Server/ReactiveUIMcp.Server.csproj
```

### Claude Desktop

Config example:

```json
{
  "mcpServers": {
    "reactiveui-mcp-server": {
      "command": "dotnet-reactiveui-mcp-server"
    }
  }
}
```

Config for running from source:

```json
{
  "mcpServers": {
    "reactiveui-mcp-server": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/ReactiveUI-mcp/ReactiveUIMcp.Server/src/ReactiveUIMcp.Server/ReactiveUIMcp.Server.csproj"
      ]
    }
  }
}
```

## Build from source

From the repository root:

```bash
dotnet build src/ReactiveUIMcpServer.slnx
```

Run the tests:

```bash
dotnet test src/ReactiveUIMcp.Tests/ReactiveUIMcp.Tests.csproj
```

Run the server directly:

```bash
dotnet run --project src/ReactiveUIMcp.Server/ReactiveUIMcp.Server.csproj
```

## Project structure

```text
ReactiveUIMcp.Server/
├── README.md
├── design.md
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── testconfig.json
├── .mcp/
│   └── server.json
└── src/
    ├── ReactiveUIMcpServer.slnx
    ├── ReactiveUIMcp.Server/
    ├── ReactiveUIMcp.Core/
    ├── ReactiveUIMcp.Knowledge/
    └── ReactiveUIMcp.Tests/
```

## Current implementation status

Implemented:
- net10.0 MCP server host
- stdio transport
- catalog/recommend/review/compare/prompt tools
- project blueprint and migration-plan tools
- wizard-based solution planning via `reactiveui_create_solution`
- optional on-disk solution generation from wizard selections
- read-only catalog resources including project-generation guidance
- scaffold, test-project, and migration prompts
- embedded knowledge catalog with ecosystem manifests including ReactiveUI.Testing
- TUnit test project with passing tests

Not yet implemented:
- advanced code-shape linting over parsed C# syntax trees
- full per-platform production-ready generated UI code instead of starter placeholders for some endpoints

## Validation

Current local verification completed:
- solution build passes
- TUnit tests pass

## MCP metadata

Server metadata file:
- `.mcp/server.json`

Current working identifiers:
- MCP server name: `io.github.chrispulman/reactiveui-mcp-server`
- package id: `CP.ReactiveUI.Mcp.Server`
- version: `0.1.0`

## Notes for future publishing

Before publishing the package, update:
- package metadata in `src/ReactiveUIMcp.Server/ReactiveUIMcp.Server.csproj`
- version in `.mcp/server.json`
- install badge links if the package id or version changes

## License

MIT License — see `LICENSE`.

---

**CP.ReactiveUI.Mcp.Server** - Empowering Agentic Automation with Reactive Technology ⚡🏭
