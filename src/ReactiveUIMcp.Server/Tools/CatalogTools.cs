
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
                HasRepositoryInventory = manifest.Inventory is not null,
            }),
        });
    }

    /// <summary>
    /// Lists the current ReactiveUI organization source repositories with full capability inventories.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <returns>A JSON payload containing the current source-repository inventories.</returns>
    [McpServerTool(Name = "reactiveui_repository_inventory_list"), Description("List the current ReactiveUI source repositories and their application/library feature, function, option, package, generator, compatibility, and migration inventories.")]
    public static string ListRepositoryInventories(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var manifests = catalog.GetAll()
            .Where(static manifest => manifest.Inventory is not null)
            .OrderBy(static manifest => manifest.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return JsonOutput.Serialize(new
        {
            Count = manifests.Length,
            Policies = new
            {
                SourceGenerators = "Prefer the relevant source generator whenever it provides the required static functionality; use manual/runtime APIs only for documented unsupported or dynamic cases.",
                ReactiveFoundation = "Prefer ReactiveUI.Primitives. Use ReactiveUI.Primitives.Reactive only when full System.Reactive Unit, IScheduler, Subject, namespace, or public-API compatibility is required.",
            },
            Repositories = manifests,
        });
    }

    /// <summary>
    /// Gets the full source-repository inventory for one manifest identifier.
    /// </summary>
    /// <param name="catalog">The knowledge catalog service.</param>
    /// <param name="id">The stable manifest identifier.</param>
    /// <returns>A JSON representation of the source-repository manifest.</returns>
    [McpServerTool(Name = "reactiveui_repository_inventory_get"), Description("Get the complete feature, function, option, package, source-generator, compatibility, and migration inventory for one current ReactiveUI source repository.")]
    public static string GetRepositoryInventory(
        IKnowledgeCatalog catalog,
        [Description("Repository manifest id such as reactiveui-core, reactiveui-primitives, sextant, or splat-di-sourcegenerator.")] string id)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var manifest = catalog.GetById(id) ?? throw new InvalidOperationException($"Unknown manifest id '{id}'.");
        if (manifest.Inventory is null)
        {
            throw new InvalidOperationException($"Manifest '{id}' is not a current source-repository inventory.");
        }

        return JsonOutput.Serialize(manifest);
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
