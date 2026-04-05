namespace ReactiveUIMcp.Server.Tools;

using ModelContextProtocol.Server;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using ReactiveUIMcp.Server.Serialization;
using System.ComponentModel;

/// <summary>
/// Wizard-oriented MCP tools for creating new ReactiveUI solutions.
/// </summary>
[McpServerToolType]
public sealed class SolutionWizardTools
{
    /// <summary>
    /// Runs the Create ReactiveUI Solution wizard as a multi-step MCP tool.
    /// </summary>
    [McpServerTool(Name = "/CreateReactiveUISolution"), Description("Create a wizard-like ReactiveUI solution plan supporting multiple UI endpoints, Splat DI provider choice, companion libraries, settings stores, common app features, validation, and scaffolded views/viewmodels. Optionally generates the solution on disk when generateFiles is true.")]
    public static string CreateReactiveUiSolution(
        IReactiveUiGuidanceService guidanceService,
        IReactiveUiSolutionScaffolder scaffolder,
        [Description("Current wizard step. Use 1/start for the first call, then 2/di, 3/features, 4/storage, 5/application, 6/views, 7/blueprint, 8/migration, or 9/complete.")] string? step = null,
        [Description("Desired solution name.")] string? solutionName = null,
        [Description("Comma-separated UI endpoints.")] string? uiEndpoints = null,
        [Description("The selected Splat DI provider, such as Splat.Microsoft.Extensions.DependencyInjection.")] string? diProvider = null,
        [Description("Comma-separated ReactiveUI or companion features.")] string? additionalFeatures = null,
        [Description("Settings store option.")] string? settingsStore = null,
        [Description("Comma-separated application features.")] string? applicationFeatures = null,
        [Description("Comma-separated prime colors or theme colors.")] string? primaryColors = null,
        [Description("Validation mode such as ReactiveUI.Validation.")] string? validationMode = null,
        [Description("Pipe-delimited endpoint:view list mapping.")] string? viewsByEndpoint = null,
        [Description("Root directory where the generated solution should be created when generateFiles is true.")] string? outputRoot = null,
        [Description("Set true with step 9/complete to create the solution on disk.")] bool generateFiles = false)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);
        ArgumentNullException.ThrowIfNull(scaffolder);

        var request = CreateReactiveUiSolutionWizardRequest.FromStrings(
            step,
            solutionName,
            uiEndpoints,
            diProvider,
            additionalFeatures,
            settingsStore,
            applicationFeatures,
            primaryColors,
            validationMode,
            viewsByEndpoint,
            outputRoot,
            generateFiles);

        var response = guidanceService.CreateReactiveUiSolutionWizard(request);
        if (response.IsComplete && generateFiles)
        {
            var generated = scaffolder.Generate(request);
            response = response with
            {
                Summary = response.Summary + $" Generated solution files under {generated.OutputPath}.",
                GeneratedSolution = generated,
            };
        }

        return JsonOutput.Serialize(response);
    }
}
