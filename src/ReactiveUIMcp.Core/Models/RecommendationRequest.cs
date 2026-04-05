namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes the desired application shape for stack recommendations or prompt generation.
/// </summary>
public sealed record RecommendationRequest(
    string? Platform,
    string? AppKind,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> ExistingLibraries)
{
    /// <summary>
    /// Creates a request from delimited string values.
    /// </summary>
    /// <param name="platform">The target platform.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">A comma, semicolon, or pipe delimited feature list.</param>
    /// <param name="constraints">A comma, semicolon, or pipe delimited constraint list.</param>
    /// <param name="existingLibraries">A comma, semicolon, or pipe delimited library list.</param>
    /// <returns>A parsed request.</returns>
    public static RecommendationRequest FromStrings(
        string? platform,
        string? appKind,
        string? features,
        string? constraints,
        string? existingLibraries) =>
        new(
            platform,
            appKind,
            Split(features),
            Split(constraints),
            Split(existingLibraries));

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value
                .Split([',', ';', '|', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
}
