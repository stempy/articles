//#:property RuntimeIdentifier=linux-x64

/// <summary>
/// Context information passed to content processors
/// </summary>
public record ProcessorContext
{
    public required FileInfo SourceFile { get; init; }
    public required DirectoryInfo SourceRoot { get; init; }
    public required Config GlobalConfig { get; init; }
    public required ContentTypeConfig TypeConfig { get; init; }
    public required int FileIndex { get; init; }
    public Dictionary<string, object> ProcessorConfig { get; init; } = new();
}
