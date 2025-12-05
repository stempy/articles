using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;
using HandlebarsDotNet;
using Spectre.Console;

Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..")));

// Configuration
var config = Helpers.LoadConfig("convert_config_generic.yml");
var sourceRoot = new DirectoryInfo(config.Source.RootDir);

if (!sourceRoot.Exists)
{
    AnsiConsole.MarkupLine("[red]✗ Error:[/] Source directory '{0}' not found", sourceRoot.FullName);
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

AnsiConsole.MarkupLine("[bold cyan]╭─────────────────────────────────────────────────────╮[/]");
AnsiConsole.MarkupLine("[bold cyan]│[/] [bold white]ConvertMarkdown Static Site Generator[/]        [bold cyan]│[/]");
AnsiConsole.MarkupLine("[bold cyan]╰─────────────────────────────────────────────────────╯[/]");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[cyan]ℹ[/] Found [yellow]{0}[/] markdown files to convert", mdFiles.Count);
AnsiConsole.MarkupLine("[cyan]ℹ[/] Source: [dim]{0}[/]", sourceRoot.FullName);
AnsiConsole.MarkupLine("[cyan]ℹ[/] Output: [dim]{0}[/]", config.Output.RootDir);
AnsiConsole.WriteLine();

var converted = 0;

for (var index = 0; index < mdFiles.Count; index++)
{
    try
    {
        var mdFile = mdFiles[index];
        var relativePath = Path.GetRelativePath(sourceRoot.FullName, mdFile.FullName);
        AnsiConsole.MarkupLine("[dim][[{0}/{1}]][/] [white]{2}[/]", index + 1, mdFiles.Count, relativePath);

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
            AnsiConsole.MarkupLine("  [red]✗ ERROR:[/] No processor found for {0}", mdFile.Name);
            continue;
        }

        AnsiConsole.MarkupLine("  [yellow]→[/] Type: [cyan]{0}[/] [dim](Processor: {1})[/]", contentType, processor.ProcessorType);

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

        var relativeOutput = Path.GetRelativePath(Directory.GetCurrentDirectory(), outputFile.FullName);
        AnsiConsole.MarkupLine("  [green]✓[/] [dim]{0}[/]", relativeOutput);
        AnsiConsole.WriteLine();

        converted++;
    }
    catch (Exception e)
    {
        AnsiConsole.MarkupLine("  [red]✗ ERROR:[/] {0}", e.Message.EscapeMarkup());
        AnsiConsole.MarkupLine("  [dim]{0}[/]", e.StackTrace?.EscapeMarkup() ?? "");
        AnsiConsole.WriteLine();
    }
}

// Summary
var failed = mdFiles.Count - converted;
var table = new Table()
    .Border(TableBorder.Rounded)
    .AddColumn(new TableColumn("[bold cyan]Summary[/]").Centered());

table.AddRow($"[green]✓ Converted:[/] [white]{converted}[/] files");
if (failed > 0)
{
    table.AddRow($"[red]✗ Failed:[/] [white]{failed}[/] files");
}
table.AddRow($"[cyan]Total:[/] [white]{mdFiles.Count}[/] files");

AnsiConsole.Write(table);
AnsiConsole.WriteLine();

// Copy included paths from content types
AnsiConsole.MarkupLine("[cyan]ℹ[/] Copying static assets...");
Helpers.CopyIncludedPaths(config, sourceRoot);
AnsiConsole.MarkupLine("[green]✓[/] [bold]Conversion complete![/]");
AnsiConsole.WriteLine();

