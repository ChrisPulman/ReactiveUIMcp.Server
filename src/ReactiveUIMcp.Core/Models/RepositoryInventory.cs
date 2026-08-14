namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes the application/library surface of one current ReactiveUI source repository.
/// </summary>
public sealed record RepositoryInventory(
    IReadOnlyList<string> ApplicationTypes,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Functions,
    IReadOnlyList<string> Options,
    IReadOnlyList<string> PackageSelection,
    IReadOnlyList<string> SourceGeneratorGuidance,
    IReadOnlyList<string> CompatibilityNotes,
    IReadOnlyList<string> MigrationGuidance);
