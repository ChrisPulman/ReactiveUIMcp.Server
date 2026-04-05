namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes a wizard question surfaced by the ReactiveUI solution creation tool.
/// </summary>
public sealed record WizardQuestion(
    string Id,
    string Prompt,
    string InputType,
    bool MultiSelect,
    IReadOnlyList<string> Options,
    string? Example = null,
    string? Notes = null);
