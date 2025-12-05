//#:property RuntimeIdentifier=linux-x64

public record OutputConfig
{
    public string RootDir { get; init; } = "w";
    public bool PreserveStructure { get; init; } = true;
}
