
using ModelContextProtocol.Server;
using ReactiveUIMcp.Core.Abstractions;
using ReactiveUIMcp.Server.Serialization;
using System.ComponentModel;

namespace ReactiveUIMcp.Server.Tools;
/// <summary>
/// MCP tools for searching and inspecting the ReactiveUI knowledge catalog.
/// </summary>
[McpServerToolType]
public sealed class CatalogTools
{
    /// <summary>
    /// Lists all known ecosystem areas.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <returns>A JSON payload describing all manifests.</returns>
    [McpServerTool(Name = "reactiveui_catalog_list"), Description("List all known ReactiveUI ecosystem areas, platforms, and companion libraries.")]
    public static string ListCatalog(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return JsonOutput.Serialize(new
        {
            Count = catalog.GetAll().Count,
            Items = catalog.GetAll().Select(static manifest => new
            {
                manifest.Id,
                manifest.DisplayName,
                manifest.Category,
                manifest.Summary,
            }),
        });
    }

    /// <summary>
    /// Searches the catalog by text and optional category.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <param name="query">Optional free-form search text.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>A JSON payload containing the matching manifests.</returns>
    [McpServerTool(Name = "reactiveui_catalog_search"), Description("Search the ReactiveUI catalog by platform, package, or keyword.")]
    public static string SearchCatalog(
        IKnowledgeCatalog catalog,
        [Description("Optional free-form search text such as 'maui akavache' or 'binding source generators'.")] string? query = null,
        [Description("Optional category filter such as platform, core, networking, persistence, validation, collections, or infrastructure.")] string? category = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return JsonOutput.Serialize(new
        {
            Query = query,
            Category = category,
            Results = catalog.Search(query, category).Select(static manifest => new
            {
                manifest.Id,
                manifest.DisplayName,
                manifest.Category,
                manifest.NuGetPackages,
                manifest.Summary,
            }),
        });
    }

    /// <summary>
    /// Gets a single manifest by identifier.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <param name="id">The stable manifest identifier.</param>
    /// <returns>A JSON representation of the manifest.</returns>
    [McpServerTool(Name = "reactiveui_catalog_get"), Description("Get detailed guidance for one ReactiveUI ecosystem area by its stable id.")]
    public static string GetManifest(
        IKnowledgeCatalog catalog,
        [Description("The manifest id, such as reactiveui-core, reactiveui-maui, akavache, or dynamicdata.")] string id)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var manifest = catalog.GetById(id) ?? throw new InvalidOperationException($"Unknown manifest id '{id}'.");
        return JsonOutput.Serialize(manifest);
    }
}
