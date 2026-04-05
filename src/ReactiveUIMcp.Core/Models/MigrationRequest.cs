namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Describes a legacy project migration target for the ReactiveUI MCP server.
/// </summary>
public sealed record MigrationRequest(
    string? Platform,
    string? ProjectType,
    IReadOnlyList<string> CurrentPackages,
    IReadOnlyList<string> UpgradeGoals,
    IReadOnlyList<string> Constraints)
{
    /// <summary>
    /// Creates a request from delimited string values.
    /// </summary>
    /// <param name="platform">The target platform.</param>
    /// <param name="projectType">The project type, such as app, library, or test project.</param>
    /// <param name="currentPackages">Delimited list of currently used packages.</param>
    /// <param name="upgradeGoals">Delimited list of migration goals.</param>
    /// <param name="constraints">Delimited list of constraints.</param>
    /// <returns>A parsed migration request.</returns>
    public static MigrationRequest FromStrings(
        string? platform,
        string? projectType,
        string? currentPackages,
        string? upgradeGoals,
        string? constraints) =>
        new(
            platform,
            projectType,
            Split(currentPackages),
            Split(upgradeGoals),
            Split(constraints));

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value
                .Split([',', ';', '|', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
}
