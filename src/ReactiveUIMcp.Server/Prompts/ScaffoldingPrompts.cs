
using ModelContextProtocol.Server;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using System.ComponentModel;

namespace ReactiveUIMcp.Server.Prompts;
/// <summary>
/// MCP prompts for generating ReactiveUI-family code with ecosystem-aware constraints.
/// </summary>
[McpServerPromptType]
public sealed class ScaffoldingPrompts
{
    /// <summary>
    /// Builds a prompt for generating new ReactiveUI implementation code.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">Desired features.</param>
    /// <param name="constraints">Constraints for the generated code.</param>
    /// <param name="existingLibraries">Existing libraries that must be respected.</param>
    /// <returns>A generated prompt string.</returns>
    [McpServerPrompt(Name = "create_reactiveui_scaffold"), Description("Create a detailed prompt for another AI coding agent to generate ReactiveUI code that follows current ecosystem guidance.")]
    public static string CreateReactiveUiScaffold(
        IReactiveUiGuidanceService guidanceService,
        [Description("Target platform such as WPF, WinForms, Blazor, MAUI, WinUI, AndroidX, Avalonia, or Uno.")] string? platform = null,
        [Description("Application kind such as desktop app, mobile app, web client, library, or test project.")] string? appKind = null,
        [Description("Comma-separated desired features.")] string? features = null,
        [Description("Comma-separated constraints.")] string? constraints = null,
        [Description("Comma-separated existing libraries that must be preserved.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(platform, appKind, features, constraints, existingLibraries);
        return guidanceService.CreateScaffoldPrompt(request);
    }

    /// <summary>
    /// Builds a prompt specialized for new ReactiveUI test project generation.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">Optional application platform under test.</param>
    /// <param name="features">Desired test features.</param>
    /// <returns>A generated prompt string.</returns>
    [McpServerPrompt(Name = "create_reactiveui_test_project"), Description("Create a prompt for generating a new ReactiveUI test project using the latest ReactiveUI.Testing guidance.")]
    public static string CreateReactiveUiTestProject(
        IReactiveUiGuidanceService guidanceService,
        [Description("Optional application platform under test, such as WPF, MAUI, or WinUI.")] string? platform = null,
        [Description("Comma-separated desired test features such as scheduler tests, command tests, or integration tests.")] string? features = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(platform, "test project", features, null, "ReactiveUI.Testing");
        return guidanceService.CreateScaffoldPrompt(request);
    }

    /// <summary>
    /// Builds a prompt specialized for upgrading legacy ReactiveUI projects.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="platform">The target platform.</param>
    /// <param name="currentPackages">Currently used packages.</param>
    /// <param name="upgradeGoals">Upgrade goals.</param>
    /// <returns>A generated migration prompt string.</returns>
    [McpServerPrompt(Name = "migrate_legacy_reactiveui_project"), Description("Create a prompt for upgrading a legacy ReactiveUI application or test project to the latest recommended packages and patterns.")]
    public static string MigrateLegacyReactiveUiProject(
        IReactiveUiGuidanceService guidanceService,
        [Description("Target platform such as WPF, MAUI, WinUI, Avalonia, or test project.")] string? platform = null,
        [Description("Comma-separated current packages such as ReactiveUI.Fody, ReactiveUI.Testing, or DynamicData.")] string? currentPackages = null,
        [Description("Comma-separated upgrade goals such as source generators, test migration, or async observable adoption.")] string? upgradeGoals = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var migration = guidanceService.CreateMigrationPlan(MigrationRequest.FromStrings(platform, "app", currentPackages, upgradeGoals, null));
        return $"Summary: {migration.Summary}\n\nPackage Actions:\n- {string.Join("\n- ", migration.PackageActions)}\n\nCode Actions:\n- {string.Join("\n- ", migration.CodeActions)}\n\nTest Actions:\n- {string.Join("\n- ", migration.TestActions)}\n\nValidation:\n- {string.Join("\n- ", migration.ValidationSteps)}";
    }
}
