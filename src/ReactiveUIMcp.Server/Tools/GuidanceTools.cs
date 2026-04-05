namespace ReactiveUIMcp.Server.Tools;

using ModelContextProtocol.Server;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using ReactiveUIMcp.Server.Serialization;
using System.ComponentModel;

/// <summary>
/// MCP tools for generating recommendations and reviewing ReactiveUI plans.
/// </summary>
[McpServerToolType]
public sealed class GuidanceTools
{
    /// <summary>
    /// Produces a recommended package and pattern set for a target stack.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">Comma-separated features.</param>
    /// <param name="constraints">Comma-separated constraints.</param>
    /// <param name="existingLibraries">Comma-separated existing libraries.</param>
    /// <returns>A structured recommendation payload.</returns>
    [McpServerTool(Name = "reactiveui_recommend"), Description("Recommend ReactiveUI packages and implementation patterns for a requested platform and feature set.")]
    public static string Recommend(
        IReactiveUiGuidanceService guidanceService,
        [Description("Target platform such as WPF, WinForms, Blazor, MAUI, WinUI, AndroidX, Avalonia, or Uno.")] string? platform = null,
        [Description("Application kind such as desktop app, mobile app, web client, library, or test project.")] string? appKind = null,
        [Description("Comma-separated features such as validation, offline cache, REST API, dynamic collections, project generation, or ReactiveUI.Testing.")] string? features = null,
        [Description("Comma-separated constraints such as trimming, NativeAOT, offline, or performance.")] string? constraints = null,
        [Description("Comma-separated existing libraries already chosen for the project.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(platform, appKind, features, constraints, existingLibraries);
        return JsonOutput.Serialize(guidanceService.Recommend(request));
    }

    /// <summary>
    /// Reviews a plan or AI prompt against known ReactiveUI guidance.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="planText">The proposed plan or generated-code instruction text.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="libraries">Optional library list.</param>
    /// <returns>A JSON review payload.</returns>
    [McpServerTool(Name = "reactiveui_review_plan"), Description("Review a proposed implementation plan or AI-generated guidance for ReactiveUI best-practice issues.")]
    public static string ReviewPlan(
        IReactiveUiGuidanceService guidanceService,
        [Description("The proposed plan or generated-code instructions to review.")] string planText,
        [Description("Optional target platform.")] string? platform = null,
        [Description("Optional comma-separated library list.")] string? libraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        return JsonOutput.Serialize(guidanceService.ReviewPlan(platform, libraries, planText));
    }

    /// <summary>
    /// Compares two ecosystem areas side-by-side.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="leftId">The left manifest identifier.</param>
    /// <param name="rightId">The right manifest identifier.</param>
    /// <returns>A JSON comparison payload.</returns>
    [McpServerTool(Name = "reactiveui_compare"), Description("Compare two ReactiveUI ecosystem areas such as MAUI vs AndroidX or SourceCache vs SourceList-related guidance.")]
    public static string Compare(
        IReactiveUiGuidanceService guidanceService,
        [Description("The left manifest id.")] string leftId,
        [Description("The right manifest id.")] string rightId)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        return JsonOutput.Serialize(guidanceService.Compare(leftId, rightId));
    }

    /// <summary>
    /// Builds a reusable prompt for another AI code generator.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">Comma-separated feature list.</param>
    /// <param name="constraints">Comma-separated constraints.</param>
    /// <param name="existingLibraries">Comma-separated existing libraries.</param>
    /// <returns>A scaffold prompt string.</returns>
    [McpServerTool(Name = "reactiveui_scaffold_prompt"), Description("Create a high-quality prompt for an AI coding agent to generate ReactiveUI code following the recommended ecosystem guidance.")]
    public static string CreateScaffoldPrompt(
        IReactiveUiGuidanceService guidanceService,
        [Description("Target platform such as WPF, MAUI, WinUI, Avalonia, Uno, or test project.")] string? platform = null,
        [Description("Application kind such as desktop app, mobile app, web client, service-backed app, or test project.")] string? appKind = null,
        [Description("Comma-separated desired features.")] string? features = null,
        [Description("Comma-separated constraints.")] string? constraints = null,
        [Description("Comma-separated existing libraries.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(platform, appKind, features, constraints, existingLibraries);
        return guidanceService.CreateScaffoldPrompt(request);
    }

    /// <summary>
    /// Produces a project-generation blueprint for a new ReactiveUI project.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">Desired features.</param>
    /// <param name="constraints">Constraints.</param>
    /// <param name="existingLibraries">Existing libraries that must be respected.</param>
    /// <returns>A JSON project blueprint.</returns>
    [McpServerTool(Name = "reactiveui_project_blueprint"), Description("Create a project-generation blueprint for a new ReactiveUI application, library, or test project using the best current packages and patterns.")]
    public static string CreateProjectBlueprint(
        IReactiveUiGuidanceService guidanceService,
        [Description("Target platform such as WPF, WinForms, Blazor, MAUI, WinUI, AndroidX, Avalonia, Uno, or general.")] string? platform = null,
        [Description("Application kind such as desktop app, mobile app, library, or test project.")] string? appKind = null,
        [Description("Comma-separated desired features such as project generation, validation, offline cache, REST API, dynamic collections, or ReactiveUI.Testing.")] string? features = null,
        [Description("Comma-separated constraints such as trimming, NativeAOT, offline, or performance.")] string? constraints = null,
        [Description("Comma-separated existing libraries.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(platform, appKind, features, constraints, existingLibraries);
        return JsonOutput.Serialize(guidanceService.CreateProjectBlueprint(request));
    }

    /// <summary>
    /// Produces a migration plan for legacy ReactiveUI applications or tests.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="projectType">The project type such as app, library, or test project.</param>
    /// <param name="currentPackages">Comma-separated currently used packages.</param>
    /// <param name="upgradeGoals">Comma-separated upgrade goals.</param>
    /// <param name="constraints">Comma-separated constraints.</param>
    /// <returns>A JSON migration plan.</returns>
    [McpServerTool(Name = "reactiveui_migration_plan"), Description("Create a migration plan for legacy ReactiveUI applications, ReactiveUI.Fody projects, and outdated ReactiveUI.Testing-based test projects.")]
    public static string CreateMigrationPlan(
        IReactiveUiGuidanceService guidanceService,
        [Description("Target platform such as WPF, WinForms, Blazor, MAUI, WinUI, AndroidX, Avalonia, Uno, or general.")] string? platform = null,
        [Description("Project type such as app, library, or test project.")] string? projectType = null,
        [Description("Comma-separated currently used packages such as ReactiveUI.Fody, ReactiveUI.Testing, DynamicData, or Akavache.")] string? currentPackages = null,
        [Description("Comma-separated upgrade goals such as source generators, legacy upgrade, test migration, or async observable adoption.")] string? upgradeGoals = null,
        [Description("Comma-separated constraints.")] string? constraints = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = MigrationRequest.FromStrings(platform, projectType, currentPackages, upgradeGoals, constraints);
        return JsonOutput.Serialize(guidanceService.CreateMigrationPlan(request));
    }
}
