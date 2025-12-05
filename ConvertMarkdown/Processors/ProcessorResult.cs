//#:property RuntimeIdentifier=linux-x64

/// <summary>
/// Result returned by content processors
/// </summary>
public record ProcessorResult
{
    public Dictionary<string, object> TemplateData { get; init; } = new();
    public string? ProcessedHtml { get; init; }
    public bool SkipMarkdownConversion { get; init; } = false;
}
