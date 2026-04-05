namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Identifies the source material that informed a manifest or recommendation.
/// </summary>
/// <param name="Title">A human-readable source title.</param>
/// <param name="Url">The canonical source URL.</param>
/// <param name="Notes">Optional notes about how the source informed the guidance.</param>
public sealed record SourceReference(string Title, string Url, string? Notes = null);
