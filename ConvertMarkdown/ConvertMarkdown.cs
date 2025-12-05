//#:property RuntimeIdentifier=linux-x64

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;
using HandlebarsDotNet;

Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..")));

// Configuration
var config = Helpers.LoadConfig("convert_config_generic.yml");
var sourceRoot = new DirectoryInfo(config.Source.RootDir);

if (!sourceRoot.Exists)
{
    Console.WriteLine($"Error: Source directory '{sourceRoot.FullName}' not found");
    return;
}

// Setup Handlebars
var handlebars = Handlebars.Create(new HandlebarsConfiguration
{
    FileSystem = new CustomFileSystem(config.Source.TemplatesDir)
});

// Register Handlebars helpers
Helpers.RegisterHandlebarsHelpers(handlebars);

// Setup processor registry
var registry = new ProcessorRegistry();
registry.Register(new IndexPageProcessor());
registry.Register(new SoftwareListProcessor());
registry.Register(new StandardArticleProcessor());

// Find all markdown files
var mdFiles = Helpers.FindMarkdownFiles(sourceRoot, config);
Console.WriteLine($"Found {mdFiles.Count} markdown files to convert");
Console.WriteLine($"Source directory: {sourceRoot.FullName}");
Console.WriteLine($"Output directory: {config.Output.RootDir}");
Console.WriteLine();

var converted = 0;

for (var index = 0; index < mdFiles.Count; index++)
{
    try
    {
        var mdFile = mdFiles[index];
        Console.WriteLine($"Converting: {Path.GetRelativePath(sourceRoot.FullName, mdFile.FullName)}");

        // Read markdown file
        var content = File.ReadAllText(mdFile.FullName);

        // Parse frontmatter and body
        var (frontmatter, body, rawFrontmatter) = Helpers.ParseFrontmatter(content);

        // Ensure there's always a title - fallback to formatted filename
        if (string.IsNullOrWhiteSpace(frontmatter.Title))
        {
            frontmatter.Title = Helpers.FormatFilenameAsTitle(mdFile.Name);
        }

        // Determine content type (for template/CSS config)
        var (contentType, contentTypeConfig) = Helpers.DetermineContentType(mdFile, config, content);

        // Get processor-specific config (empty for now, will be populated from YAML later)
        var processorConfig = new Dictionary<string, object>();

        // Build processor context
        var context = new ProcessorContext
        {
            SourceFile = mdFile,
            SourceRoot = sourceRoot,
            GlobalConfig = config,
            TypeConfig = contentTypeConfig,
            FileIndex = index,
            ProcessorConfig = processorConfig
        };

        // Find matching processor
        var processor = registry.FindProcessor(mdFile, content, context);
        if (processor == null)
        {
            Console.WriteLine($"  ERROR: No processor found for {mdFile.Name}");
            continue;
        }

        Console.WriteLine($"  Type: {contentType} (Processor: {processor.ProcessorType})");

        // Process gallery liquid tags BEFORE processor runs
        // This prevents the gallery HTML from being wrapped in <p> tags
        body = Gallery.ProcessGalleries(body, rawFrontmatter);

        // Process content with processor
        var result = processor.Process(body, rawFrontmatter, context);

        // Prepare template data
        var templateData = new Dictionary<string, object>
        {
            ["title"] = frontmatter.Title,
            ["css_files"] = contentTypeConfig.CssFiles,
            ["footer_text"] = config.Defaults?.FooterText ?? "Articles"
        };

        // Merge processor results
        foreach (var kvp in result.TemplateData)
        {
            templateData[kvp.Key] = kvp.Value;
        }

        // Override title if processor provided one
        if (!string.IsNullOrWhiteSpace((string?)result.TemplateData.GetValueOrDefault("title")))
        {
            templateData["title"] = result.TemplateData["title"];
        }

        // Add footer text for special content types
        if (processor.ProcessorType == "index_page")
        {
            templateData["footer_text"] = $"Compiled <span>{result.TemplateData.GetValueOrDefault("date", "November 2025")}</span> · New collections ship as they are ready";
        }
        else if (processor.ProcessorType == "software_list")
        {
            // Use frontmatter title if software list didn't extract one
            if (string.IsNullOrWhiteSpace((string?)templateData["title"]))
            {
                templateData["title"] = frontmatter.Title;
                templateData["header_title"] = SoftwareListProcessor.FormatSoftwareListTitle(frontmatter.Title);
            }
            templateData["footer_text"] = "Compiled <span>November 2025</span> · A tribute to software that endures";
        }

        // Render template with Handlebars
        // Check frontmatter first, then fall back to content type config
        var templateName = frontmatter.Template?.Replace(".html", ".hbs")
                        ?? contentTypeConfig.Template?.Replace(".html", ".hbs")
                        ?? "default.hbs";
        var template = Helpers.LoadHandlebarsTemplate(handlebars, templateName, config);
        var html = template(templateData);

        // Determine output path
        var outputFile = Helpers.GetOutputPath(mdFile, sourceRoot, config, contentTypeConfig);

        // Create output directory
        outputFile.Directory?.Create();

        // Write HTML file
        File.WriteAllText(outputFile.FullName, html);

        Console.WriteLine($"  → {outputFile.FullName}");
        Console.WriteLine();

        converted++;
    }
    catch (Exception e)
    {
        Console.WriteLine($"  ERROR: {e.Message}");
        Console.WriteLine($"  {e.StackTrace}");
        Console.WriteLine();
    }
}

Console.WriteLine($"Conversion complete! Converted {converted} of {mdFiles.Count} files.");

// Copy included paths from content types
Helpers.CopyIncludedPaths(config, sourceRoot);

