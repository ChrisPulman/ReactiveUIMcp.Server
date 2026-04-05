namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// A structured result for a guidance review.
/// </summary>
public sealed record ReviewResult(
    string Summary,
    IReadOnlyList<ReviewFinding> Findings,
    IReadOnlyList<string> RecommendedManifestIds);
