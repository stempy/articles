//#:property RuntimeIdentifier=linux-x64

/// <summary>
/// Registry for managing content processors
/// </summary>
public class ProcessorRegistry
{
    private readonly List<IContentProcessor> _processors = new();

    /// <summary>
    /// Register a content processor
    /// Processors are automatically sorted by priority (descending)
    /// </summary>
    public void Register(IContentProcessor processor)
    {
        _processors.Add(processor);
        // Sort by priority descending (higher priority first)
        _processors.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// Find the first processor that can handle the given file
    /// </summary>
    public IContentProcessor? FindProcessor(FileInfo file, string content, ProcessorContext context)
    {
        return _processors.FirstOrDefault(p => p.CanProcess(file, content, context));
    }
}
