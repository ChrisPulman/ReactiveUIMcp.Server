namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// A structured recommendation assembled from one or more manifests.
/// </summary>
public sealed record RecommendationResult(
    string Summary,
    IReadOnlyList<string> SelectedManifestIds,
    IReadOnlyList<string> SuggestedPackages,
    IReadOnlyList<string> RecommendedPatterns,
    IReadOnlyList<string> AvoidPatterns,
    IReadOnlyList<string> SetupSteps,
    IReadOnlyList<string> CommonPitfalls,
    IReadOnlyList<string> RelatedLibraries);
