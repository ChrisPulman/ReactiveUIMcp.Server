namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// A structured plan for generating a new ReactiveUI project using the recommended packages and patterns.
/// </summary>
public sealed record ProjectBlueprintResult(
    string Summary,
    IReadOnlyList<string> SuggestedPackages,
    IReadOnlyList<string> SetupSteps,
    IReadOnlyList<string> ProjectStructureHints,
    IReadOnlyList<string> CodeGenerationRules,
    IReadOnlyList<string> TestProjectRecommendations);
