namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Side-by-side comparison of two ecosystem areas.
/// </summary>
public sealed record ComparisonResult(
    string Summary,
    string LeftId,
    string RightId,
    IReadOnlyList<string> LeftPackages,
    IReadOnlyList<string> RightPackages,
    IReadOnlyList<string> LeftPatterns,
    IReadOnlyList<string> RightPatterns,
    IReadOnlyList<string> Tradeoffs);
