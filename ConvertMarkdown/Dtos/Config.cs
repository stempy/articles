//#:property RuntimeIdentifier=linux-x64

// ===== Data Models =====

record Config
{
    public SourceConfig Source { get; init; } = new();
    public OutputConfig Output { get; init; } = new();
    public Dictionary<string, ContentTypeConfig> ContentTypes { get; init; } = new();
    public PathsConfig? Paths { get; init; }
    public List<string>? AccentColors { get; init; }
    public DefaultsConfig? Defaults { get; init; }
}
