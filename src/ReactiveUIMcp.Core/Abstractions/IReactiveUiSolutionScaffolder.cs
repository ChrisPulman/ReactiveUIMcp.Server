namespace ReactiveUIMcp.Core.Abstractions;

using ReactiveUIMcp.Core.Models;

/// <summary>
/// Generates an on-disk ReactiveUI solution from wizard selections.
/// </summary>
public interface IReactiveUiSolutionScaffolder
{
    /// <summary>
    /// Generates a solution skeleton on disk.
    /// </summary>
    /// <param name="request">The completed wizard request.</param>
    /// <returns>A summary of the generated files.</returns>
    GeneratedReactiveUiSolutionResult Generate(CreateReactiveUiSolutionWizardRequest request);
}
