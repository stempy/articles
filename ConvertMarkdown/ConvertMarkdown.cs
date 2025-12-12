using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;
using HandlebarsDotNet;
using Spectre.Console;

Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..")));

// Configuration
var config = Helpers.LoadConfig("convertmarkdown_config.yml");
var sourceRoot = new DirectoryInfo(config.Source.RootDir);

if (!sourceRoot.Exists)
{
    AnsiConsole.MarkupLine("[red]✗ Error:[/] Source directory '{0}' not found", sourceRoot.FullName);
    return;
}

// Setup Handlebars
var fileSystem = new CustomFileSystem(config.Source.TemplatesDir);
var handlebars = Handlebars.Create(new HandlebarsConfiguration
{
    FileSystem = fileSystem
});

// Register Handlebars helpers
Helpers.RegisterHandlebarsHelpers(handlebars);

// Register layout partials
fileSystem.RegisterLayoutPartials(handlebars);

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

var infoTable = new Table()
    .Border(TableBorder.None)
    .HideHeaders()
    .AddColumn(new TableColumn("Label"))
    .AddColumn(new TableColumn("Value"));

infoTable.AddRow("[cyan]Found:[/]", $"[yellow]{mdFiles.Count}[/] markdown files");
infoTable.AddRow("[cyan]Source:[/]", $"[dim]{sourceRoot.FullName}[/]");
infoTable.AddRow("[cyan]Output:[/]", $"[dim]{config.Output.RootDir}[/]");

AnsiConsole.Write(infoTable);
AnsiConsole.WriteLine();

// Create processing table
var processTable = new Table()
    .Border(TableBorder.Rounded)
    .BorderColor(Color.Grey)
    .AddColumn(new TableColumn("[bold]#[/]").Centered().Width(5))
    .AddColumn(new TableColumn("[bold]Source File[/]").LeftAligned())
    .AddColumn(new TableColumn("[bold]Type[/]").Centered().Width(12))
    .AddColumn(new TableColumn("[bold]Status[/]").Centered().Width(8))
    .AddColumn(new TableColumn("[bold]Output[/]").LeftAligned());

var converted = 0;

for (var index = 0; index < mdFiles.Count; index++)
{
    try
    {
        var mdFile = mdFiles[index];
        var relativePath = Path.GetRelativePath(sourceRoot.FullName, mdFile.FullName);

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
            processTable.AddRow(
                $"[dim]{index + 1}[/]",
                $"[white]{relativePath}[/]",
                "[dim]—[/]",
                "[red]✗[/]",
                "[red]No processor found[/]"
            );
            continue;
        }

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

        // Generate CSS link tags
        var cssLinks = string.Join("\n    ", contentTypeConfig.CssFiles.Select(css =>
            $"<link rel=\"stylesheet\" href=\"{css}\">"));
        templateData["css_links"] = cssLinks;

        // Merge processor results
        foreach (var kvp in result.TemplateData)
        {
            templateData[kvp.Key] = kvp.Value;
        }

        // Extract and format date (after processor merge, so we take precedence)
        var dateStr = Helpers.ExtractDate(rawFrontmatter, mdFile);
        if (!string.IsNullOrWhiteSpace(dateStr))
        {
            templateData["date"] = Helpers.FormatDate(dateStr);
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

        processTable.AddRow(
            $"[dim]{index + 1}[/]",
            $"[white]{relativePath}[/]",
            $"[cyan]{contentType}[/]",
            "[green]✓[/]",
            $"[dim]{relativeOutput}[/]"
        );

        converted++;
    }
    catch (Exception e)
    {
        var relativePath = Path.GetRelativePath(sourceRoot.FullName, mdFiles[index].FullName);
        processTable.AddRow(
            $"[dim]{index + 1}[/]",
            $"[white]{relativePath}[/]",
            "[dim]—[/]",
            "[red]✗[/]",
            $"[red]{e.Message.EscapeMarkup()}[/]"
        );
    }
}

// Display processing results
AnsiConsole.Write(processTable);
AnsiConsole.WriteLine();

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
var assetTable = new Table()
    .Border(TableBorder.None)
    .HideHeaders()
    .AddColumn(new TableColumn("Icon"))
    .AddColumn(new TableColumn("Message"));

assetTable.AddRow("[cyan]ℹ[/]", "Copying static assets...");
AnsiConsole.Write(assetTable);

Helpers.CopyIncludedPaths(config, sourceRoot);

// Copy CSS folder to output root
var cssSourceDir = Path.Combine(Directory.GetCurrentDirectory(), "css");
var cssDestDir = Path.Combine(config.Output.RootDir, "css");

if (Directory.Exists(cssSourceDir))
{
    AnsiConsole.MarkupLine("  [dim]Copying CSS directory: {0} → {1}[/]", cssSourceDir, cssDestDir);
    Helpers.CopyDirectory(cssSourceDir, cssDestDir);
}
else
{
    AnsiConsole.MarkupLine("  [yellow]⚠ Warning:[/] CSS directory not found: {0}", cssSourceDir);
}

assetTable = new Table()
    .Border(TableBorder.None)
    .HideHeaders()
    .AddColumn(new TableColumn("Icon"))
    .AddColumn(new TableColumn("Message"));

assetTable.AddRow("[green]✓[/]", "[bold green]Conversion complete![/]");
AnsiConsole.Write(assetTable);
AnsiConsole.WriteLine();

