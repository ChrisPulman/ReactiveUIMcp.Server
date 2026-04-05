
using ModelContextProtocol.Server;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Server.Serialization;
using System.ComponentModel;

namespace ReactiveUIMcp.Server.Resources;
/// <summary>
/// Read-only MCP resources exposing the ecosystem catalog and individual guidance packs.
/// </summary>
[McpServerResourceType]
public sealed class CatalogResources
{
    /// <summary>
    /// Gets a catalog overview resource.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <returns>The serialized catalog overview.</returns>
    [McpServerResource(UriTemplate = "reactiveui://catalog", Name = "ReactiveUI Catalog", MimeType = "application/json")]
    [Description("Read-only overview of the ReactiveUI ecosystem catalog.")]
    public static string GetCatalog(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return JsonOutput.Serialize(new
        {
            Count = catalog.GetAll().Count,
            Categories = catalog.GetAll()
                .Select(static manifest => manifest.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase),
            Items = catalog.GetAll().Select(static manifest => new
            {
                manifest.Id,
                manifest.DisplayName,
                manifest.Category,
            }),
        });
    }

    /// <summary>
    /// Gets a specific ecosystem manifest by templated resource id.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <param name="id">The manifest id.</param>
    /// <returns>The serialized manifest resource.</returns>
    [McpServerResource(UriTemplate = "reactiveui://ecosystem/{id}", Name = "ReactiveUI Ecosystem Manifest", MimeType = "application/json")]
    [Description("Read-only manifest for a specific ReactiveUI ecosystem area.")]
    public static string GetManifest(IKnowledgeCatalog catalog, string id)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var manifest = catalog.GetById(id) ?? throw new InvalidOperationException($"Unknown manifest id '{id}'.");
        return JsonOutput.Serialize(manifest);
    }

    /// <summary>
    /// Gets a focused resource describing project-generation guidance.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <returns>A JSON resource tailored to project generation scenarios.</returns>
    [McpServerResource(UriTemplate = "reactiveui://best-practices/project-generation", Name = "ReactiveUI Project Generation Guidance", MimeType = "application/json")]
    [Description("Read-only guidance focused on generating new ReactiveUI applications and test projects.")]
    public static string GetProjectGenerationGuidance(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var ids = new[]
        {
            "reactiveui-core",
            "reactiveui-sourcegenerators",
            "reactiveui-binding-sourcegenerators",
            "extensions",
            "dynamicdata",
            "reactiveui-testing"
        };

        var manifests = ids
            .Select(catalog.GetById)
            .Where(static manifest => manifest is not null)
            .Cast<object>()
            .ToArray();

        return JsonOutput.Serialize(new
        {
            Purpose = "Project generation and modernization guidance for new ReactiveUI apps and tests.",
            Manifests = manifests,
        });
    }
}
