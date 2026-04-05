namespace ReactiveUIMcp.Core.Models;

/// <summary>
/// Represents one finding produced while reviewing a plan or generated code strategy.
/// </summary>
public sealed record ReviewFinding(string Severity, string Rule, string Message, string Recommendation);
