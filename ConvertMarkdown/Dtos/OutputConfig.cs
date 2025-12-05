//#:property RuntimeIdentifier=linux-x64

record OutputConfig
{
    public string RootDir { get; init; } = "w";
    public bool PreserveStructure { get; init; } = true;
}
