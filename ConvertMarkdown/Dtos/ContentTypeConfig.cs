//#:property RuntimeIdentifier=linux-x64

record ContentTypeConfig
{
    public string? SourcePath { get; init; }
    public string? Template { get; init; }
    public List<string> CssFiles { get; init; } = new();
    public string? BackLink { get; init; }
    public string? OutputSubdir { get; init; }
    public bool IsIndex { get; init; }
    public bool IsSoftwareList { get; init; }
    public List<string>? IncludePaths { get; init; }
}
