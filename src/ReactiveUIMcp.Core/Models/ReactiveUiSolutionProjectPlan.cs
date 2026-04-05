namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes one project that should exist in a generated ReactiveUI solution.
/// </summary>
public sealed record ReactiveUiSolutionProjectPlan(
    string Name,
    string Purpose,
    string Template,
    IReadOnlyList<string> TargetEndpoints,
    IReadOnlyList<string> RecommendedPackages,
    IReadOnlyList<string> Notes);
