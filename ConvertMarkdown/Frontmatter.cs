//#:property RuntimeIdentifier=linux-x64

record Frontmatter
{
    public string Title { get; set; } = "";
    public string? Excerpt { get; init; }
    public string? Date { get; init; }
    public List<string>? Tags { get; init; }
}
