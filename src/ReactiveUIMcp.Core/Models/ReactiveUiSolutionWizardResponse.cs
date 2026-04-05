namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Represents a wizard step or final blueprint for creating a ReactiveUI solution.
/// </summary>
public sealed record ReactiveUiSolutionWizardResponse(
    string CurrentStep,
    string NextStep,
    bool IsComplete,
    string Summary,
    IReadOnlyList<WizardQuestion> Questions,
    IReadOnlyDictionary<string, string> CurrentSelections,
    ProjectBlueprintResult? Blueprint = null,
    IReadOnlyList<ReactiveUiSolutionProjectPlan>? Projects = null,
    IReadOnlyList<ReactiveUiViewScaffold>? ViewScaffolds = null,
    GeneratedReactiveUiSolutionResult? GeneratedSolution = null);
