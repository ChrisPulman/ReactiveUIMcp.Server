using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
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

        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddSingleton<IKnowledgeCatalog, EmbeddedKnowledgeCatalog>();
        builder.Services.AddSingleton<IReactiveUiGuidanceService, ReactiveUiGuidanceService>();
        builder.Services.AddSingleton<IReactiveUiSolutionScaffolder, ReactiveUiSolutionScaffolder>();

        builder.Services.AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "io.github.chrispulman/reactiveui-mcp-server",
                    Version = "0.1.0",
                    Title = "ReactiveUI MCP Server",
                    Description = "ReactiveUI ecosystem guidance for AI-assisted code generation, review, project creation, and migration planning.",
                    WebsiteUrl = "https://github.com/ChrisPulman/ReactiveUIMcp.Server",
                };
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
    public static async Task Main(string[] args)
    {
        await CreateHost(args).RunAsync();
    }
}
