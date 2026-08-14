# ReactiveUI MCP Server

<!-- mcp-name: io.github.chrispulman/reactiveui-mcp-server -->

Give your coding agent current, opinionated ReactiveUI ecosystem guidance while it designs, generates, reviews, or migrates .NET applications and libraries.

The server runs locally over MCP `stdio`. It gives tools such as Codex, Claude Code, GitHub Copilot, VS Code, Visual Studio, and other MCP clients a searchable inventory of ReactiveUI repositories, packages, APIs, configuration choices, source generators, compatibility constraints, and migration paths.

[![VS Code - Install ReactiveUI MCP](https://img.shields.io/badge/VS_Code-Install_ReactiveUI_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=reactiveui-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.ReactiveUI.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)
[![VS Code Insiders - Install ReactiveUI MCP](https://img.shields.io/badge/VS_Code_Insiders-Install_ReactiveUI_MCP-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=reactiveui-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.ReactiveUI.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D&quality=insiders)
[![Visual Studio - Install ReactiveUI MCP](https://img.shields.io/badge/Visual_Studio-Install_ReactiveUI_MCP-5C2D91?style=flat-square&logo=visualstudio&logoColor=white)](https://vs-open.link/mcp-install?%7B%22name%22%3A%22CP.ReactiveUI.Mcp.Server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.ReactiveUI.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)

## What it does

ReactiveUI MCP helps an agent answer practical questions such as:

- Which ReactiveUI and companion packages should this application or library reference?
- Which source generator should replace handwritten properties, commands, bindings, or DI registration?
- How should ReactiveUI be initialized for WPF, WinForms, WinUI, MAUI, Blazor, Avalonia, or Uno?
- Which Splat resolver, DI adapter, logging adapter, drawing package, or monitoring integration fits this project?
- How should a project migrate from `System.Reactive` to `ReactiveUI.Primitives`?
- When is a `.Reactive` compatibility package actually required?
- How should a legacy Fody or older ReactiveUI project be modernized?
- Does a proposed architecture follow current activation, disposal, binding, validation, routing, caching, networking, and testing guidance?
- What would a new multi-platform ReactiveUI solution look like, and can the starter files be generated?

The server provides structured answers rather than relying on whichever library details happen to be present in the model's training data.

## Guidance defaults

The MCP server deliberately applies these defaults:

1. Prefer `ReactiveUI.SourceGenerators` for supported reactive properties, observable-as-property helpers, commands, view models, and views.
2. Prefer the generators included with `ReactiveUI.Binding` for supported static observation and binding paths.
3. Prefer `Splat.DependencyInjection.SourceGenerator` for statically analyzable service graphs. Use manual registration for dynamic discovery, plugins, unsupported open generics, or intentional container-specific behavior.
4. Prefer Refit's generated clients over its opt-in reflection fallback.
5. Prefer `ReactiveUI.Primitives` over `System.Reactive` for new code.
6. Use `ReactiveUI.Primitives.Reactive` and matching `.Reactive` platform packages only when full `System.Reactive` compatibility is required at an API or package boundary.

These are recommendations, not hidden rewrites. The tools return the packages, trade-offs, risks, and migration actions so the calling agent can explain its choices.

## Quick start

Install the [.NET 10 SDK or later](https://dotnet.microsoft.com/download), then register this local `stdio` command in your MCP client:

```text
Command: dnx
Arguments: CP.ReactiveUI.Mcp.Server@0.* --yes
```

The client launches and manages the process. You do not need to start the server in a separate terminal.

### Codex

Add the server from a terminal:

```powershell
codex mcp add reactiveui -- dnx CP.ReactiveUI.Mcp.Server@0.* --yes
```

Confirm that Codex can see it:

```powershell
codex mcp list
```

You can instead add it to `~/.codex/config.toml`, or to `.codex/config.toml` in a trusted project:

```toml
[mcp_servers.reactiveui]
command = "dnx"
args = ["CP.ReactiveUI.Mcp.Server@0.*", "--yes"]
```

The Codex CLI, Codex IDE extension, and ChatGPT desktop app share this MCP configuration. See the [official Codex MCP documentation](https://developers.openai.com/codex/mcp/).

### Claude Code

Register the server for your user account:

```powershell
claude mcp add --scope user --transport stdio reactiveui -- dnx CP.ReactiveUI.Mcp.Server@0.* --yes
```

Verify the connection with:

```powershell
claude mcp list
claude mcp get reactiveui
```

For a configuration shared with a project, add `.mcp.json`:

```json
{
  "mcpServers": {
    "reactiveui": {
      "command": "dnx",
      "args": ["CP.ReactiveUI.Mcp.Server@0.*", "--yes"]
    }
  }
}
```

Run Claude Code in the project, approve the project server when prompted, and use `/mcp` to inspect its status. See the [official Claude Code MCP documentation](https://code.claude.com/docs/en/mcp).

### GitHub Copilot in VS Code

Use one of the install buttons above, run `MCP: Add Server` from the command palette, or create `.vscode/mcp.json`:

```json
{
  "servers": {
    "reactiveui": {
      "type": "stdio",
      "command": "dnx",
      "args": ["CP.ReactiveUI.Mcp.Server@0.*", "--yes"]
    }
  }
}
```

After saving the file:

1. Run `MCP: List Servers` and start `reactiveui` if it is not already running.
2. Open Copilot Chat and select **Agent** mode.
3. Open the tools picker and confirm that the ReactiveUI tools are enabled.

Organization-managed Copilot accounts may require the **MCP servers in Copilot** policy to be enabled. See the [GitHub Copilot MCP documentation](https://docs.github.com/en/copilot/how-tos/provide-context/use-mcp-in-your-ide/extend-copilot-chat-with-mcp?tool=vscode).

### GitHub Copilot in Visual Studio

In a supported Visual Studio version:

1. Open **GitHub Copilot Chat** and select **Agent** mode.
2. Open the tools picker, select **+**, and choose **Add custom MCP server**.
3. Select `stdio`.
4. Set the command to `dnx`.
5. Set the arguments to `CP.ReactiveUI.Mcp.Server@0.* --yes`.
6. Save, start the server, and enable its tools.

See Microsoft's [Visual Studio MCP server guide](https://learn.microsoft.com/en-us/visualstudio/ide/mcp-servers?view=vs-2022) for supported versions, trust prompts, and configuration locations.

### Other MCP clients

For any client that supports local `stdio` servers, register these values using that client's configuration format:

| Setting | Value |
| --- | --- |
| Name | `reactiveui` |
| Transport | `stdio` |
| Command | `dnx` |
| Arguments | `CP.ReactiveUI.Mcp.Server@0.*`, `--yes` |

Configuration property names vary between MCP clients. Some use `servers`; others use `mcpServers`. Use the schema documented by your client rather than copying a different client's wrapper object.

## Running from source

Clone this repository and replace the packaged command with:

```text
Command: dotnet
Arguments: run --project <absolute-path>/src/ReactiveUIMcp.Server/CP.ReactiveUIMcp.Server.csproj
```

For example, a generic `stdio` server entry would use:

```json
{
  "command": "dotnet",
  "args": [
    "run",
    "--project",
    "C:/Projects/ReactiveUIMcp.Server/src/ReactiveUIMcp.Server/CP.ReactiveUIMcp.Server.csproj"
  ]
}
```

Build and test the repository with:

```powershell
dotnet build src/ReactiveUIMcpServer.slnx -c Release
dotnet test src/ReactiveUIMcp.Tests/ReactiveUIMcp.Tests.csproj -c Release
```

## How to use it

Once the server is connected, ask your agent to use the ReactiveUI MCP tools explicitly or let the agent select them when relevant.

Example requests:

```text
Use the ReactiveUI MCP server to recommend packages and startup patterns for a new Avalonia desktop application with validation, Refit, and Akavache.
```

```text
Create a migration plan from System.Reactive to ReactiveUI.Primitives for this WPF library. Preserve System.Reactive only if its public API requires full compatibility.
```

```text
Show me the complete Splat package inventory, explain the Global and Instance GenericFirst resolver trade-off, and prefer generated DI registrations where possible.
```

```text
Review this ReactiveUI implementation plan for activation, disposal, binding, scheduler, service-location, and testing problems.
```

```text
Create a ReactiveUI solution plan with WPF and MAUI endpoints, shared view models, generated bindings, Splat DI, Refit, Akavache, validation, and TUnit tests.
```

## Available tools

| Tool | Use it for |
| --- | --- |
| `reactiveui_catalog_list` | List known platforms and ecosystem areas. |
| `reactiveui_catalog_search` | Search packages, APIs, patterns, platforms, or keywords. |
| `reactiveui_catalog_get` | Retrieve one complete catalog manifest by stable ID. |
| `reactiveui_repository_inventory_list` | List the 15 current source repositories and their complete inventories. |
| `reactiveui_repository_inventory_get` | Retrieve features, functions, options, packages, compatibility, and migration guidance for one repository. |
| `reactiveui_recommend` | Recommend packages and implementation patterns for a platform and feature set. |
| `reactiveui_review_plan` | Review proposed or generated guidance for ReactiveUI-specific problems. |
| `reactiveui_compare` | Compare two ecosystem areas or implementation choices. |
| `reactiveui_scaffold_prompt` | Produce a detailed implementation prompt for another coding agent. |
| `reactiveui_project_blueprint` | Design a new application, library, or test-project structure. |
| `reactiveui_migration_plan` | Plan Fody, legacy ReactiveUI, testing, or System.Reactive-to-Primitives migration. |
| `reactiveui_create_solution` | Run a multi-step solution wizard and optionally generate starter files on disk. |

The server also exposes reusable prompts for application scaffolding, TUnit test-project creation, and legacy migration.

## Available resources

| Resource | Contents |
| --- | --- |
| `reactiveui://catalog` | Complete ecosystem catalog. |
| `reactiveui://ecosystem/{id}` | One catalog manifest by stable ID. |
| `reactiveui://source-repositories` | Current source repositories and complete inventories. |
| `reactiveui://best-practices/project-generation` | Focused project-generation and modernization guidance. |

## Repository coverage

The current source-repository inventory contains:

- Akavache
- Fusillade
- Maui.Plugins.Popup
- Primitives
- punchclock
- ReactiveUI
- ReactiveUI.Avalonia
- ReactiveUI.Binding.SourceGenerators
- ReactiveUI.SourceGenerators
- ReactiveUI.Uno
- ReactiveUI.Validation
- refit
- Sextant
- Splat
- Splat.DI.SourceGenerator

Each record includes supported application and library types, features, callable APIs, configuration options, package selection, source-generator guidance, compatibility notes, and migration guidance.

The broader catalog also covers platform packages, DynamicData, ReactiveUI.Testing, and legacy migration topics that are useful when generating or modernizing applications.

## Splat package coverage

Splat is represented as a full package family rather than only the base `Splat` package:

| Area | Packages |
| --- | --- |
| Core and composition | `Splat`, `Splat.Core`, `Splat.Builder`, `Splat.Logging` |
| Drawing and images | `Splat.Drawing`, `Splat.SkiaSharp` |
| DI adapters | `Splat.Autofac`, `Splat.DryIoc`, `Splat.Microsoft.Extensions.DependencyInjection`, `Splat.Ninject`, `Splat.Prism`, `Splat.SimpleInjector` |
| Logging adapters | `Splat.Log4Net`, `Splat.Microsoft.Extensions.Logging`, `Splat.NLog`, `Splat.Serilog` |
| Monitoring adapters | `Splat.ApplicationInsights`, `Splat.ApplicationInsightsV3`, `Splat.Exceptionless`, `Splat.Raygun` |
| Generated DI companion | `Splat.DependencyInjection.SourceGenerator` |

The generated DI companion is maintained in the separate `Splat.DI.SourceGenerator` repository. The MCP server recommends it first for static graphs while retaining runtime Splat registration for genuinely dynamic cases.

## Troubleshooting

### The server does not appear

- Confirm that `dnx` is available from the same environment that launches your agent.
- Run the client's MCP list or status command.
- Check that the package ID is exactly `CP.ReactiveUI.Mcp.Server`.
- Check that the argument list keeps `CP.ReactiveUI.Mcp.Server@0.*` and `--yes` as separate arguments.
- Restart or reload the client after changing its configuration.

### The server starts and immediately fails

- Update the .NET SDK.
- Try the exact packaged command from a terminal to expose package-resolution errors.
- If running from source, use an absolute project path and confirm that it points to `CP.ReactiveUIMcp.Server.csproj`.

### Copilot cannot use MCP tools

Check whether an organization or enterprise policy disables MCP servers, then confirm the tools are enabled in Agent mode.

### An agent gives generic ReactiveUI advice without using the server

Ask it directly: `Use the ReactiveUI MCP server to ...`. You can also name a specific tool, such as `reactiveui_recommend`, `reactiveui_repository_inventory_get`, or `reactiveui_migration_plan`.

## Package and server identity

- MCP server: `io.github.chrispulman/reactiveui-mcp-server`
- NuGet package: `CP.ReactiveUI.Mcp.Server`
- Transport: local `stdio`
- License: MIT

See [.mcp/server.json](.mcp/server.json) for registry metadata.

## License

This project is licensed under the [MIT License](LICENSE).
