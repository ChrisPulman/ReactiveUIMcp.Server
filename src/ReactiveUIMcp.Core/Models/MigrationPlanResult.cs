namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// A structured migration plan for upgrading legacy ReactiveUI applications or tests.
/// </summary>
public sealed record MigrationPlanResult(
    string Summary,
    IReadOnlyList<string> PackageActions,
    IReadOnlyList<string> CodeActions,
    IReadOnlyList<string> TestActions,
    IReadOnlyList<string> ValidationSteps,
    IReadOnlyList<string> Risks);
