
using System.Text.Json;

namespace ReactiveUIMcp.Server.Serialization;
/// <summary>
/// Shared JSON formatting helpers for MCP tool and resource responses.
/// </summary>
internal static class JsonOutput
{
    private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Serializes a payload to indented JSON.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized JSON string.</returns>
    public static string Serialize(object value) => JsonSerializer.Serialize(value, s_options);
}
