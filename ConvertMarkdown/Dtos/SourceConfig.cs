//#:property RuntimeIdentifier=linux-x64

public record SourceConfig
{
    public string RootDir { get; init; } = "docs";
    public string TemplatesDir { get; init; } = "templates";
    public List<string>? ExcludeDirs { get; init; }
    public List<string>? ExcludeFiles { get; init; }
}
