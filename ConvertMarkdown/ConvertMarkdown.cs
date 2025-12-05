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

        // Determine content type (pass content for detection)
        var (contentType, contentTypeConfig) = Helpers.DetermineContentType(mdFile, config, content);
        Console.WriteLine($"  Type: {contentType}");

        // Prepare template data
        var templateData = new Dictionary<string, object>
        {
            ["title"] = frontmatter.Title,
            ["css_files"] = contentTypeConfig.CssFiles,
            ["footer_text"] = config.Defaults?.FooterText ?? "Articles"
        };

        // Process based on content type
        string html;
        if (contentTypeConfig.IsIndex)
        {
            var indexData = Helpers.ParseIndexContent(body, config);
            foreach (var kvp in indexData)
                templateData[kvp.Key] = kvp.Value;

            templateData["footer_text"] = $"Compiled <span>{indexData.GetValueOrDefault("date", "November 2025")}</span> · New collections ship as they are ready";
        }
        else if (contentTypeConfig.IsSoftwareList)
        {
            var listData = SoftwareHelpers.ParseSoftwareList(body, config);
            foreach (var kvp in listData)
                templateData[kvp.Key] = kvp.Value;

            // Use frontmatter title if software list didn't extract one
            if (string.IsNullOrWhiteSpace((string)templateData["title"]))
            {
                templateData["title"] = frontmatter.Title;
                templateData["header_title"] = SoftwareHelpers.FormatSoftwareListTitle(frontmatter.Title);
            }

            templateData["footer_text"] = "Compiled <span>November 2025</span> · A tribute to software that endures";
        }
        else
        {
            // Process gallery liquid tags BEFORE markdown conversion
            // This prevents the gallery HTML from being wrapped in <p> tags
            body = Gallery.ProcessGalleries(body, rawFrontmatter);

            // Standard article - use Markdig for robust markdown conversion
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseAutoLinks()
                .UseBootstrap()
                .UseEmojiAndSmiley()
                .UseTaskLists()
                .Build();

            var bodyHtml = Markdown.ToHtml(body, pipeline);

            var accentColor = Helpers.GetAccentColor(index, config);

            templateData["accent_color"] = accentColor;
            templateData["back_link"] = contentTypeConfig.BackLink ?? "../index.html";
            templateData["date"] = Helpers.FormatDate(frontmatter.Date);
            templateData["excerpt"] = frontmatter.Excerpt ?? "";
            templateData["body_content"] = bodyHtml;
        }

        // Render template with Handlebars
        var templateName = contentTypeConfig.Template?.Replace(".html", ".hbs") ?? "default.hbs";
        var template = Helpers.LoadHandlebarsTemplate(handlebars, templateName, config);
        html = template(templateData);

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

