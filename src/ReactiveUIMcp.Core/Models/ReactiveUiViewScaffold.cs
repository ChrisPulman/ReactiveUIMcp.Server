namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes a scaffolded view and matching view model recommendation.
/// </summary>
public sealed record ReactiveUiViewScaffold(
    string Endpoint,
    string ViewName,
    string ViewModelName,
    IReadOnlyList<string> SuggestedFiles,
    IReadOnlyList<string> Notes);
