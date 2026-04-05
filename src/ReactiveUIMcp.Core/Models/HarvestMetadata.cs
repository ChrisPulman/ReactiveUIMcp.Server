namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes when and how a manifest was harvested or curated.
/// </summary>
/// <param name="HarvestedAtUtc">The UTC timestamp describing the harvest snapshot.</param>
/// <param name="SourceSummary">A short description of the research basis.</param>
/// <param name="Notes">Optional implementation notes.</param>
public sealed record HarvestMetadata(DateTime HarvestedAtUtc, string SourceSummary, string? Notes = null);
