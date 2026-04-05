namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes the files and notes produced when a ReactiveUI solution is generated on disk.
/// </summary>
public sealed record GeneratedReactiveUiSolutionResult(
    string OutputPath,
    IReadOnlyList<string> CreatedFiles,
    IReadOnlyList<string> Notes);
