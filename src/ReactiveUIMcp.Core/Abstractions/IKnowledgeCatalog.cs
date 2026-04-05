
using ReactiveUIMcp.Core.Models;

namespace ReactiveUIMcp.Core.Abstractions;
/// <summary>
/// Provides access to the harvested ReactiveUI ecosystem knowledge catalog.
/// </summary>
public interface IKnowledgeCatalog
{
    /// <summary>
    /// Gets all manifests in the catalog.
    /// </summary>
    /// <returns>All known ecosystem manifests.</returns>
    IReadOnlyList<EcosystemManifest> GetAll();

    /// <summary>
    /// Gets a manifest by its stable identifier.
    /// </summary>
    /// <param name="id">The manifest identifier.</param>
    /// <returns>The matching manifest if it exists; otherwise <see langword="null"/>.</returns>
    EcosystemManifest? GetById(string id);

    /// <summary>
    /// Searches the catalog by free-form text and optional category.
    /// </summary>
    /// <param name="query">Optional search text.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>The matching manifests.</returns>
    IReadOnlyList<EcosystemManifest> Search(string? query = null, string? category = null);
}
