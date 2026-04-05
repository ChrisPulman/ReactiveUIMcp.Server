using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Services;
using ReactiveUIMcp.Knowledge.Services;
using ReactiveUIMcp.Server.Prompts;
using ReactiveUIMcp.Server.Resources;
using ReactiveUIMcp.Server.Services;
using ReactiveUIMcp.Server.Tools;

namespace ReactiveUIMcp.Server;

/// <summary>
/// Entry point for the ReactiveUI MCP server host.
/// </summary>
public static class Program
{
    /// <summary>
    /// Builds the host used by the ReactiveUI MCP server.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The configured host.</returns>
    public static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

        builder.Services.AddSingleton<IKnowledgeCatalog, EmbeddedKnowledgeCatalog>();
        builder.Services.AddSingleton<IReactiveUiGuidanceService, ReactiveUiGuidanceService>();
        builder.Services.AddSingleton<IReactiveUiSolutionScaffolder, ReactiveUiSolutionScaffolder>();

        builder.Services.AddMcpServer(options => options.ServerInfo = new Implementation
        {
            Name = "reactiveui-mcp-server",
            Version = typeof(Program).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion
            ?? typeof(Program).Assembly.GetName().Version?.ToString()
            ?? "0.0.0",
            Title = "ReactiveUI MCP Server",
            Description = "ReactiveUI ecosystem guidance for AI-assisted code generation, review, project creation, and migration planning.",
            WebsiteUrl = "https://github.com/ChrisPulman/ReactiveUIMcp.Server",
            Icons =
                    [
                        new Icon
                        {
                            Source = "https://raw.githubusercontent.com/microsoft/fluentui-emoji/62ecdc0d7ca5c6df32148c169556bc8d3782fca4/assets/Gear/Flat/gear_flat.svg",
                            MimeType = "image/svg+xml",
                            Sizes = ["any"],
                            Theme = "light"
                        },
                        new Icon
                        {
                            Source = "https://raw.githubusercontent.com/microsoft/fluentui-emoji/62ecdc0d7ca5c6df32148c169556bc8d3782fca4/assets/Gear/3D/gear_3d.png",
                            MimeType = "image/png",
                            Sizes = ["256x256"]
                        }
                    ]
        })
            .WithStdioServerTransport()
            .WithTools<CatalogTools>()
            .WithTools<GuidanceTools>()
            .WithTools<SolutionWizardTools>()
            .WithResources<CatalogResources>()
            .WithPrompts<ScaffoldingPrompts>();

        return builder.Build();
    }

    /// <summary>
    /// Starts the MCP server process.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[] args) => await CreateHost(args).RunAsync();
}
