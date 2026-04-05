
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Core.Models;
using System.Text.Json;

namespace ReactiveUIMcp.Knowledge.Services;
/// <summary>
/// Loads ecosystem manifests from embedded JSON resources.
/// </summary>
public sealed class EmbeddedKnowledgeCatalog : IKnowledgeCatalog
{
    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly Lazy<IReadOnlyList<EcosystemManifest>> _manifests = new(LoadManifests);

    /// <inheritdoc />
    public IReadOnlyList<EcosystemManifest> GetAll() => _manifests.Value;

    /// <inheritdoc />
    public EcosystemManifest? GetById(string id) =>
        GetAll().FirstOrDefault(manifest => string.Equals(manifest.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public IReadOnlyList<EcosystemManifest> Search(string? query = null, string? category = null)
    {
        IEnumerable<EcosystemManifest> manifests = GetAll();

        if (!string.IsNullOrWhiteSpace(category))
        {
            manifests = manifests.Where(manifest => string.Equals(manifest.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return manifests.OrderBy(static manifest => manifest.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        var tokens = query
            .Split([' ', ',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static token => token.ToLowerInvariant())
            .ToArray();

        return manifests
            .Where(manifest => tokens.All(token => SearchText(manifest).Contains(token, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(static manifest => manifest.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<EcosystemManifest> LoadManifests()
    {
        var assembly = typeof(EmbeddedKnowledgeCatalog).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(static name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var manifests = new List<EcosystemManifest>(resourceNames.Length);
        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Unable to load embedded resource '{resourceName}'.");

            var manifest = JsonSerializer.Deserialize<EcosystemManifest>(stream, s_serializerOptions)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' did not deserialize to an ecosystem manifest.");

            manifests.Add(manifest);
        }

        return manifests;
    }

    private static string SearchText(EcosystemManifest manifest) =>
        string.Join(
            ' ',
            manifest.Id,
            manifest.DisplayName,
            manifest.Category,
            manifest.Summary,
            string.Join(' ', manifest.NuGetPackages),
            string.Join(' ', manifest.Keywords),
            string.Join(' ', manifest.RelatedLibraries));
}
