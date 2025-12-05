//#:property RuntimeIdentifier=linux-x64

using Markdig;

/// <summary>
/// Standard article processor for markdown content
/// This is the fallback processor with lowest priority
/// </summary>
public class StandardArticleProcessor : IContentProcessor
{
    public int Priority => 0; // Lowest priority - acts as fallback

    public string ProcessorType => "standard_article";

    public bool CanProcess(FileInfo file, string content, ProcessorContext context)
    {
        // Always returns true - this is the fallback processor
        return true;
    }

    public MarkdownPipelineBuilder? ConfigureMarkdownPipeline(MarkdownPipelineBuilder builder)
    {
        // Use advanced markdown extensions by default
        return builder
            .UseAdvancedExtensions()
            .UseAutoLinks()
            .UseBootstrap()
            .UseEmojiAndSmiley()
            .UseTaskLists();
    }

    public ProcessorResult Process(string body, Dictionary<string, object> rawFrontmatter, ProcessorContext context)
    {
        // Build markdown pipeline
        var pipeline = ConfigureMarkdownPipeline(new MarkdownPipelineBuilder())!.Build();

        // Convert markdown to HTML
        var bodyHtml = Markdown.ToHtml(body, pipeline);

        // Get accent color from config
        var accentColor = Helpers.GetAccentColor(context.FileIndex, context.GlobalConfig);

        // Extract date from frontmatter if present
        var date = "";
        if (rawFrontmatter.TryGetValue("date", out var dateObj))
        {
            date = Helpers.FormatDate(dateObj?.ToString());
        }

        // Extract excerpt from frontmatter if present
        var excerpt = "";
        if (rawFrontmatter.TryGetValue("excerpt", out var excerptObj))
        {
            excerpt = excerptObj?.ToString() ?? "";
        }

        return new ProcessorResult
        {
            TemplateData = new Dictionary<string, object>
            {
                ["body_content"] = bodyHtml,
                ["accent_color"] = accentColor,
                ["back_link"] = context.TypeConfig.BackLink ?? "../index.html",
                ["date"] = date,
                ["excerpt"] = excerpt
            }
        };
    }
}
