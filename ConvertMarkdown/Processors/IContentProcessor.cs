//#:property RuntimeIdentifier=linux-x64

using Markdig;

/// <summary>
/// Interface for content processors that handle different types of markdown content
/// </summary>
public interface IContentProcessor
{
    /// <summary>
    /// Priority determines order of evaluation (higher = earlier)
    /// Allows processors to override built-in processors
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Unique identifier for this processor type
    /// </summary>
    string ProcessorType { get; }

    /// <summary>
    /// Determines if this processor can handle the given content
    /// </summary>
    bool CanProcess(FileInfo file, string content, ProcessorContext context);

    /// <summary>
    /// Processes the content and returns template data
    /// </summary>
    ProcessorResult Process(string body, Dictionary<string, object> rawFrontmatter, ProcessorContext context);

    /// <summary>
    /// Configure Markdig pipeline if this processor uses markdown conversion
    /// Return null to skip markdown processing
    /// </summary>
    MarkdownPipelineBuilder? ConfigureMarkdownPipeline(MarkdownPipelineBuilder builder);
}
