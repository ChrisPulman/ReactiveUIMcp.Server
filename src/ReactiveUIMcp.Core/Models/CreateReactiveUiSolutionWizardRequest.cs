namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes the current state of the Create ReactiveUI Solution wizard.
/// </summary>
public sealed record CreateReactiveUiSolutionWizardRequest(
    string? Step,
    string? SolutionName,
    IReadOnlyList<string> UiEndpoints,
    string? DiProvider,
    IReadOnlyList<string> AdditionalFeatures,
    string? SettingsStore,
    IReadOnlyList<string> ApplicationFeatures,
    string? PrimaryColors,
    string? ValidationMode,
    string? ViewsByEndpoint,
    string? OutputRoot = null,
    bool GenerateFiles = false)
{
    /// <summary>
    /// Creates a wizard request from delimited input strings.
    /// </summary>
    public static CreateReactiveUiSolutionWizardRequest FromStrings(
        string? step,
        string? solutionName,
        string? uiEndpoints,
        string? diProvider,
        string? additionalFeatures,
        string? settingsStore,
        string? applicationFeatures,
        string? primaryColors,
        string? validationMode,
        string? viewsByEndpoint,
        string? outputRoot,
        bool generateFiles) =>
        new(
            step,
            solutionName,
            Split(uiEndpoints),
            diProvider,
            Split(additionalFeatures),
            settingsStore,
            Split(applicationFeatures),
            primaryColors,
            validationMode,
            viewsByEndpoint,
            outputRoot,
            generateFiles);

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value
                .Split([',', ';', '|', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
}
