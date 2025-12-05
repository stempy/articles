//#:property RuntimeIdentifier=linux-x64

/// <summary>
/// Configuration for a content processor
/// </summary>
public record ProcessorConfigDto
{
    public string Type { get; init; } = "";
    public int Priority { get; init; } = 0;
    public DetectorConfig? Detector { get; init; }
    public Dictionary<string, object> Config { get; init; } = new();
}

/// <summary>
/// Detector configuration for matching files to processors
/// </summary>
public record DetectorConfig
{
    public string? Filename { get; init; }
    public string? SourcePath { get; init; }
    public string? ContentPattern { get; init; }
}
