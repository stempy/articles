#:property PublishAot=false
//#:property RuntimeIdentifier=linux-x64
#:package YamlDotNet@16.2.0
#:package Markdig@0.38.0
#:package Handlebars.Net@2.1.6

using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;
using HandlebarsDotNet;

// Configuration
var config = LoadConfig("convert_config_generic.yml");
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
RegisterHandlebarsHelpers(handlebars);

// Find all markdown files
var mdFiles = FindMarkdownFiles(sourceRoot, config);
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
        var (frontmatter, body, rawFrontmatter) = ParseFrontmatter(content);

        // Ensure there's always a title - fallback to formatted filename
        if (string.IsNullOrWhiteSpace(frontmatter.Title))
        {
            frontmatter.Title = FormatFilenameAsTitle(mdFile.Name);
        }

        // Determine content type (pass content for detection)
        var (contentType, contentTypeConfig) = DetermineContentType(mdFile, config, content);
        Console.WriteLine($"  Type: {contentType}");

        // Prepare template data
        var templateData = new Dictionary<string, object>
        {
            ["title"] = frontmatter.Title,
            ["css_files"] = contentTypeConfig.CssFiles,
            ["footer_text"] = config.Defaults?.FooterText ?? "Stempy Articles"
        };

        // Process based on content type
        string html;
        if (contentTypeConfig.IsIndex)
        {
            var indexData = ParseIndexContent(body, config);
            foreach (var kvp in indexData)
                templateData[kvp.Key] = kvp.Value;

            templateData["footer_text"] = $"Compiled <span>{indexData.GetValueOrDefault("date", "November 2025")}</span> · New collections ship as they are ready";
        }
        else if (contentTypeConfig.IsSoftwareList)
        {
            var listData = ParseSoftwareList(body, config);
            foreach (var kvp in listData)
                templateData[kvp.Key] = kvp.Value;

            // Use frontmatter title if software list didn't extract one
            if (string.IsNullOrWhiteSpace((string)templateData["title"]))
            {
                templateData["title"] = frontmatter.Title;
                templateData["header_title"] = FormatSoftwareListTitle(frontmatter.Title);
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

            var accentColor = GetAccentColor(index, config);

            templateData["accent_color"] = accentColor;
            templateData["back_link"] = contentTypeConfig.BackLink ?? "../index.html";
            templateData["date"] = FormatDate(frontmatter.Date);
            templateData["excerpt"] = frontmatter.Excerpt ?? "";
            templateData["body_content"] = bodyHtml;
        }

        // Render template with Handlebars
        var templateName = contentTypeConfig.Template?.Replace(".html", ".hbs") ?? "default.hbs";
        var template = LoadHandlebarsTemplate(handlebars, templateName);
        html = template(templateData);

        // Determine output path
        var outputFile = GetOutputPath(mdFile, sourceRoot, config, contentTypeConfig);

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
CopyIncludedPaths(config, sourceRoot);

// ===== Helper Functions =====

Config LoadConfig(string configFile)
{
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    var yaml = File.ReadAllText(configFile);
    return deserializer.Deserialize<Config>(yaml);
}

(Frontmatter frontmatter, string body, Dictionary<string, object> rawFrontmatter) ParseFrontmatter(string content)
{
    var frontmatter = new Frontmatter();
    var body = content;
    var rawFrontmatter = new Dictionary<string, object>();

    if (!content.StartsWith("---"))
    {
        // No frontmatter, try to extract title from first H1
        var lines = content.Split('\n');
        var newLines = new List<string>();
        var titleExtracted = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("# ") && !titleExtracted)
            {
                frontmatter.Title = line[2..].Trim();
                titleExtracted = true;
                continue;
            }
            newLines.Add(line);
        }

        return (frontmatter, string.Join('\n', newLines), rawFrontmatter);
    }

    var parts = content.Split("---", 3, StringSplitOptions.TrimEntries);
    if (parts.Length < 3)
        return (frontmatter, content, rawFrontmatter);

    var frontmatterText = parts[1];
    body = parts[2];

    try
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        frontmatter = deserializer.Deserialize<Frontmatter>(frontmatterText) ?? new Frontmatter();

        // Also deserialize as raw dictionary to preserve all fields (like gallery)
        var rawDeserializer = new DeserializerBuilder()
            .Build();

        var rawData = rawDeserializer.Deserialize<Dictionary<string, object>>(frontmatterText);
        if (rawData != null)
        {
            rawFrontmatter = rawData;
        }
    }
    catch
    {
        frontmatter = new Frontmatter();
    }

    // If frontmatter exists but has no title, try to extract from first H1 in body
    if (string.IsNullOrWhiteSpace(frontmatter.Title))
    {
        var lines = body.Split('\n');
        foreach (var line in lines)
        {
            if (line.StartsWith("# "))
            {
                frontmatter.Title = line[2..].Trim();
                break;
            }
        }
    }

    return (frontmatter, body, rawFrontmatter);
}

string FormatFilenameAsTitle(string filename)
{
    var name = Path.GetFileNameWithoutExtension(filename);
    name = name.Replace('-', ' ').Replace('_', ' ');
    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
}

(string type, ContentTypeConfig config) DetermineContentType(FileInfo file, Config config, string content)
{
    var sourceRoot = new DirectoryInfo(config.Source.RootDir);
    var relPath = Path.GetRelativePath(sourceRoot.FullName, file.FullName);

    // Check if it's index.md
    if (file.Name == "index.md")
    {
        if (config.ContentTypes.TryGetValue("index", out var indexConfig))
            return ("index", indexConfig);
    }

    // Check if it's a software list based on content
    if (IsSoftwareList(content))
    {
        if (config.ContentTypes.TryGetValue("software_list", out var softwareConfig))
            return ("software_list", softwareConfig);
    }

    // Check each content type
    foreach (var (typeName, typeConfig) in config.ContentTypes)
    {
        if (typeName is "default" or "index" or "software_list")
            continue;

        if (!string.IsNullOrWhiteSpace(typeConfig.SourcePath) &&
            relPath.Replace('\\', '/').StartsWith(typeConfig.SourcePath))
        {
            return (typeName, typeConfig);
        }
    }

    // Return default
    return ("default", config.ContentTypes.GetValueOrDefault("default", new ContentTypeConfig()));
}

bool IsSoftwareList(string content) =>
    content.Contains("| Software |") && content.Contains("| Years Active |");

string GetAccentColor(int index, Config config)
{
    var colors = config.AccentColors ?? new List<string> { "gold", "purple", "cyan" };
    return colors[index % colors.Count];
}

string FormatDate(string? dateStr)
{
    if (string.IsNullOrWhiteSpace(dateStr))
        return "";

    if (DateTime.TryParse(dateStr, out var date))
        return date.ToString("MMMM yyyy");

    return dateStr;
}

string FormatSoftwareListTitle(string title)
{
    // Format title with span for styling
    if (title.Contains("Test of Time"))
        return "Software That Stands<br>the <span>Test of Time</span>";
    else if (title.Contains("Tool Setup"))
        return "My Local <span>Tool Setup</span>";
    else
    {
        var parts = title.Split(' ');
        if (parts.Length >= 2)
        {
            var lastTwo = string.Join(' ', parts.TakeLast(2));
            var rest = string.Join(' ', parts.SkipLast(2));
            return rest + " <span>" + lastTwo + "</span>";
        }
    }
    return title;
}

Dictionary<string, object> ParseIndexContent(string body, Config config)
{
    var lines = body.Split('\n');
    var title = "Stempy Editions";
    var subtitle = "Stempy Editions";
    var headerTitle = "Collections on <span>Durable Software Development</span>";
    var intro = "Notes, research, and curated content";
    var sectionLabel = "Collections";
    var sectionTitle = "Published & Upcoming";
    var articles = new List<Dictionary<string, object>>();

    // Status to CSS class mapping
    var statusClassMap = new Dictionary<string, string>
    {
        ["Live"] = "live",
        ["In Progress"] = "soon",
        ["Soon"] = "soon",
        ["Upcoming"] = "soon"
    };

    // Accent colors
    var accentColors = config.AccentColors ?? new List<string> { "gold", "purple", "cyan" };

    Dictionary<string, object>? currentArticle = null;

    for (var i = 0; i < lines.Length; i++)
    {
        var line = lines[i].Trim();

        // Extract main title (H1)
        if (line.StartsWith("# "))
        {
            title = subtitle = line[2..].Trim();
        }
        // Extract header title (H2 - first one becomes the header)
        else if (line.StartsWith("## ") && headerTitle == "Collections on <span>Durable Software Development</span>")
        {
            headerTitle = line[3..].Trim();
        }
        // Extract section title (H2 after "Published")
        else if (line.StartsWith("## ") && (line.Contains("Published") || line.Contains("Upcoming")))
        {
            sectionTitle = line[3..].Trim();
        }
        // Extract article title (H3)
        else if (line.StartsWith("### "))
        {
            // Save previous article if exists
            if (currentArticle != null && currentArticle.ContainsKey("title"))
            {
                var articleIndex = articles.Count;
                currentArticle["accent_color"] = accentColors[articleIndex % accentColors.Count];
                currentArticle["status_class"] = statusClassMap.GetValueOrDefault((string?)currentArticle.GetValueOrDefault("status") ?? "Live", "live");
                articles.Add(currentArticle);
            }

            currentArticle = new Dictionary<string, object>
            {
                ["title"] = line[4..].Trim(),
                ["status"] = "Live",
                ["date"] = "",
                ["summary"] = "",
                ["link"] = ""
            };
        }
        // Extract status and date (bold text with "Status:")
        else if (currentArticle != null && (line.StartsWith("**Status:**") || line.StartsWith("<strong>Status:</strong>")))
        {
            var statusLine = line.Replace("**Status:**", "").Replace("<strong>Status:</strong>", "").Trim();
            var parts = statusLine.Split('|');
            if (parts.Length >= 2)
            {
                currentArticle["status"] = parts[0].Trim();
                currentArticle["date"] = parts[1].Replace("Updated", "").Trim();
            }
        }
        // Extract summary (paragraph after status)
        else if (currentArticle != null &&
                 !string.IsNullOrWhiteSpace(line) &&
                 !line.StartsWith("#") &&
                 !line.StartsWith("**") &&
                 !line.StartsWith("[") &&
                 !line.StartsWith("---") &&
                 !line.StartsWith("*") &&
                 currentArticle.ContainsKey("status") &&
                 string.IsNullOrEmpty((string)currentArticle["summary"]))
        {
            currentArticle["summary"] = line;
        }
        // Extract link
        else if (currentArticle != null && line.StartsWith("[Explore]"))
        {
            var match = Regex.Match(line, @"\[Explore\]\((.*?)\)");
            if (match.Success)
            {
                currentArticle["link"] = match.Groups[1].Value;
            }
        }
        // Extract intro text (text between H2 and first ---)
        else if (!string.IsNullOrWhiteSpace(line) &&
                 !line.StartsWith("#") &&
                 !line.StartsWith("---") &&
                 intro == "Notes, research, and curated content" &&
                 currentArticle == null)
        {
            if (!line.StartsWith("**") && !line.StartsWith("["))
            {
                intro = line;
            }
        }
    }

    // Save last article
    if (currentArticle != null && currentArticle.ContainsKey("title"))
    {
        var articleIndex = articles.Count;
        currentArticle["accent_color"] = accentColors[articleIndex % accentColors.Count];
        currentArticle["status_class"] = statusClassMap.GetValueOrDefault((string?)currentArticle.GetValueOrDefault("status") ?? "Live", "live");
        articles.Add(currentArticle);
    }

    return new Dictionary<string, object>
    {
        ["title"] = title,
        ["subtitle"] = subtitle,
        ["header_title"] = headerTitle,
        ["intro"] = intro,
        ["section_label"] = sectionLabel,
        ["section_title"] = sectionTitle,
        ["articles"] = articles,
        ["date"] = "November 2025"
    };
}

Dictionary<string, object> ParseSoftwareList(string body, Config config)
{
    var lines = body.Split('\n');
    var title = "";
    var subtitle = "A Curated Collection";
    var intro = "";
    var eraSections = new List<Dictionary<string, object>>();
    var traits = new List<Dictionary<string, object>>();

    Dictionary<string, object>? currentEra = null;
    var currentTable = new List<string>();
    var sectionDescription = new List<string>();
    var inTable = false;
    var inTraits = false;

    // Map era titles to badge classes
    var badgeMap = new Dictionary<string, string>
    {
        ["Legends"] = "legends",
        ["Veterans"] = "veterans",
        ["Established"] = "established",
        ["Mature"] = "mature",
        ["Proven"] = "proven",
        ["Rising Stars"] = "rising",
        ["Rising"] = "rising",
        ["HOST"] = "legends",
        ["VIRTUALIZATION"] = "veterans",
        ["BROWSERS"] = "established",
        [".NET STACK"] = "mature",
        ["TOOLS"] = "proven",
        ["AI"] = "rising"
    };

    // Map era titles to accent colors
    var colorMap = new Dictionary<string, string>
    {
        ["Legends"] = "gold",
        ["Veterans"] = "purple",
        ["Established"] = "cyan",
        ["Mature"] = "blue",
        ["Proven"] = "rose",
        ["Rising Stars"] = "green",
        ["Rising"] = "green",
        ["HOST"] = "gold",
        ["VIRTUALIZATION"] = "purple",
        ["BROWSERS"] = "cyan",
        [".NET STACK"] = "blue",
        ["TOOLS"] = "rose",
        ["AI"] = "green"
    };

    for (var i = 0; i < lines.Length; i++)
    {
        var line = lines[i].Trim();

        // Extract main title (H1)
        if (line.StartsWith("# "))
        {
            title = line[2..].Trim();

            // Format title with span for styling
            if (title.Contains("Test of Time"))
                title = "Software That Stands<br>the <span>Test of Time</span>";
            else if (title.Contains("Tool Setup"))
                title = "My Local <span>Tool Setup</span>";
            else
            {
                var parts = title.Split(' ');
                if (parts.Length >= 2)
                {
                    var lastTwo = string.Join(' ', parts.TakeLast(2));
                    var rest = string.Join(' ', parts.SkipLast(2));
                    title = rest + " <span>" + lastTwo + "</span>";
                }
            }
        }
        // Extract intro (first paragraph after title)
        else if (!string.IsNullOrWhiteSpace(line) &&
                 !line.StartsWith("#") &&
                 !line.StartsWith("---") &&
                 !line.StartsWith("|") &&
                 string.IsNullOrEmpty(intro) &&
                 !inTraits &&
                 currentEra == null)
        {
            intro = line;
        }
        // Extract era header (H2 with parentheses)
        else if (line.StartsWith("## ") && line.Contains("("))
        {
            // Save previous era if exists
            if (currentEra != null && currentTable.Count > 0)
            {
                currentEra["software"] = ParseSoftwareTable(currentTable);
                currentEra["description"] = BuildDescriptionHtml(sectionDescription);
                eraSections.Add(currentEra);
                currentTable.Clear();
            }

            // Reset section description for new section
            sectionDescription.Clear();

            // Parse era header - supports two formats:
            // 1. "30+ Years (Legends)" - traditional format
            // 2. "HOST (Windows 11 Host)" - badge-first format
            var headerText = line[3..].Trim();
            var parts = headerText.Split('(');
            var firstPart = parts[0].Trim();
            var secondPart = parts.Length > 1 ? parts[1].TrimEnd(')').Trim() : firstPart;

            string years, displayName, badgeName;

            // Determine which format we have
            if (firstPart.All(c => char.IsUpper(c) || char.IsWhiteSpace(c) || c == '.') || badgeMap.ContainsKey(firstPart))
            {
                // Badge-first format: "HOST (Windows 11 Host)"
                badgeName = firstPart;
                displayName = secondPart;
                years = badgeName;
            }
            else
            {
                // Traditional format: "30+ Years (Legends)"
                years = firstPart;
                displayName = secondPart;
                badgeName = secondPart;
            }

            currentEra = new Dictionary<string, object>
            {
                ["years"] = years,
                ["name"] = displayName,
                ["badge_class"] = badgeMap.GetValueOrDefault(badgeName, "legends"),
                ["color"] = colorMap.GetValueOrDefault(badgeName, "gold"),
                ["software"] = new List<Dictionary<string, object>>(),
                ["description"] = ""
            };
            inTable = false;
        }
        // Detect traits section
        else if (line.StartsWith("## ") && line.Contains("Traits"))
        {
            // Save last era
            if (currentEra != null && currentTable.Count > 0)
            {
                currentEra["software"] = ParseSoftwareTable(currentTable);
                currentEra["description"] = BuildDescriptionHtml(sectionDescription);
                eraSections.Add(currentEra);
                currentTable.Clear();
                currentEra = null;
            }
            inTraits = true;
        }
        // Parse traits as numbered list
        else if (inTraits && !string.IsNullOrWhiteSpace(line) && char.IsDigit(line[0]) && line.Contains('.') && line.Contains("**"))
        {
            var traitParts = line.Split('.', 2);
            if (traitParts.Length > 1)
            {
                var text = traitParts[1].Trim();
                if (text.Contains("**"))
                {
                    var titleMatch = Regex.Match(text, @"\*\*(.*?)\*\*");
                    var titlePart = titleMatch.Success ? titleMatch.Groups[1].Value : "";
                    var descPart = text.Contains("—") ? text.Split('—', 2)[1].Trim() : "";
                    traits.Add(new Dictionary<string, object>
                    {
                        ["title"] = titlePart,
                        ["description"] = descPart
                    });
                }
            }
        }
        // Collect table lines
        else if (line.StartsWith("|") && !line.StartsWith("|---"))
        {
            if (!inTable)
            {
                inTable = true;
                currentTable.Add(line);
            }
            else
            {
                currentTable.Add(line);
            }
        }
        else if (inTable && !line.StartsWith("|"))
        {
            inTable = false;
        }
        // Collect section description (text between H2 and table)
        else if (currentEra != null && !inTable && !inTraits && !string.IsNullOrWhiteSpace(line) && !line.StartsWith("---") && !line.StartsWith("#"))
        {
            sectionDescription.Add(line);
        }
    }

    // Save last era
    if (currentEra != null && currentTable.Count > 0)
    {
        currentEra["software"] = ParseSoftwareTable(currentTable);
        currentEra["description"] = BuildDescriptionHtml(sectionDescription);
        eraSections.Add(currentEra);
    }

    return new Dictionary<string, object>
    {
        ["title"] = title,
        ["subtitle"] = subtitle,
        ["header_title"] = title,
        ["intro"] = intro,
        ["era_sections"] = eraSections,
        ["traits"] = traits
    };
}

string BuildDescriptionHtml(List<string> descLines)
{
    if (descLines.Count == 0)
        return "";

    var htmlParts = new List<string>();
    var inList = false;

    foreach (var line in descLines)
    {
        var stripped = line.Trim();
        if (string.IsNullOrWhiteSpace(stripped))
            continue;

        // Handle bullet points
        if (stripped.StartsWith("- "))
        {
            if (!inList)
            {
                htmlParts.Add("<ul>");
                inList = true;
            }
            htmlParts.Add($"<li>{stripped[2..]}</li>");
        }
        else
        {
            if (inList)
            {
                htmlParts.Add("</ul>");
                inList = false;
            }
            // Convert bold markdown
            stripped = Regex.Replace(stripped, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
            htmlParts.Add($"<p>{stripped}</p>");
        }
    }

    if (inList)
        htmlParts.Add("</ul>");

    return string.Join("", htmlParts);
}

List<Dictionary<string, object>> ParseSoftwareTable(List<string> tableLines)
{
    var softwareList = new List<Dictionary<string, object>>();

    // Skip header row
    for (var i = 1; i < tableLines.Count; i++)
    {
        var cells = tableLines[i].Split('|').Select(c => c.Trim()).ToArray();
        if (cells.Length >= 6) // Account for empty first and last cells
        {
            var nameCell = cells[1].Replace("**", "").Trim();

            softwareList.Add(new Dictionary<string, object>
            {
                ["name"] = nameCell,
                ["year"] = cells[2].Trim(),
                ["years_active"] = cells[3].Trim(),
                ["category"] = cells[4].Trim(),
                ["description"] = cells[5].Trim(),
                ["url"] = ""
            });
        }
    }

    return softwareList;
}

List<FileInfo> FindMarkdownFiles(DirectoryInfo root, Config config)
{
    var files = new List<FileInfo>();

    foreach (var file in root.EnumerateFiles("*.md", SearchOption.AllDirectories))
    {
        if (!ShouldExclude(file, root, config))
            files.Add(file);
    }

    return files.OrderBy(f => f.FullName).ToList();
}

bool ShouldExclude(FileInfo file, DirectoryInfo root, Config config)
{
    var relPath = Path.GetRelativePath(root.FullName, file.FullName);
    var pathParts = relPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    // Check excluded directories
    foreach (var excludeDir in config.Source.ExcludeDirs ?? new List<string>())
    {
        if (pathParts.Contains(excludeDir))
            return true;
    }

    // Check excluded files (but not root-level files)
    if (pathParts.Length > 1 && (config.Source.ExcludeFiles ?? new List<string>()).Contains(file.Name))
        return true;

    return false;
}

FileInfo GetOutputPath(FileInfo sourceFile, DirectoryInfo sourceRoot, Config config, ContentTypeConfig typeConfig)
{
    var relPath = Path.GetRelativePath(sourceRoot.FullName, sourceFile.FullName);
    var outputRoot = config.Output.RootDir;

    // Remove source_path prefix if it exists
    if (!string.IsNullOrWhiteSpace(typeConfig.SourcePath))
    {
        relPath = relPath.Replace(typeConfig.SourcePath, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    // Add output subdirectory
    var outputPath = !string.IsNullOrWhiteSpace(typeConfig.OutputSubdir)
        ? Path.Combine(outputRoot, typeConfig.OutputSubdir, Path.GetDirectoryName(relPath) ?? "")
        : Path.Combine(outputRoot, Path.GetDirectoryName(relPath) ?? "");

    // Change extension to .html
    var outputFile = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(sourceFile.Name) + ".html");

    return new FileInfo(outputFile);
}

Func<object, string> LoadHandlebarsTemplate(IHandlebars handlebars, string templateName)
{
    var templatePath = Path.Combine(config.Source.TemplatesDir, templateName);
    Console.WriteLine($"  Using template: {templateName}");
    if (!File.Exists(templatePath))
    {
        // Try without .hbs extension
        templatePath = Path.Combine(config.Source.TemplatesDir, templateName.Replace(".hbs", ".html"));
    }

    if (!File.Exists(templatePath))
    {
        Console.WriteLine($"Warning: Template not found: {templateName}");
        return data =>
        {
            var dict = data as Dictionary<string, object>;
            var title = dict?.GetValueOrDefault("title", "Untitled") ?? "Untitled";
            return $"<html><body><h1>{title}</h1></body></html>";
        };
    }

    var templateContent = File.ReadAllText(templatePath);
    var compiled = handlebars.Compile(templateContent);
    return data => compiled(data);
}

void RegisterHandlebarsHelpers(IHandlebars handlebars)
{
    // Register equality helper
    handlebars.RegisterHelper("eq", (context, arguments) =>
    {
        if (arguments.Length == 2)
        {
            return Equals(arguments[0], arguments[1]);
        }
        return false;
    });

    // Register if helper (enhanced)
    handlebars.RegisterHelper("if", (output, options, context, arguments) =>
    {
        if (arguments.Length > 0 && arguments[0] is bool condition && condition)
        {
            options.Template(output, context);
        }
        else
        {
            options.Inverse(output, context);
        }
    });

    // Register increment helper (for 1-based indexing)
    handlebars.RegisterHelper("inc", (output, context, arguments) =>
    {
        if (arguments.Length > 0 && arguments[0] is int value)
        {
            output.Write((value + 1).ToString());
        }
    });
}

void CopyIncludedPaths(Config config, DirectoryInfo sourceRoot)
{
    Console.WriteLine();
    Console.WriteLine("Copying included paths...");

    var copiedCount = 0;

    foreach (var (typeName, typeConfig) in config.ContentTypes)
    {
        if (typeConfig.IncludePaths == null || typeConfig.IncludePaths.Count == 0)
            continue;

        if (string.IsNullOrWhiteSpace(typeConfig.SourcePath))
            continue;

        foreach (var includePath in typeConfig.IncludePaths)
        {
            var sourcePath = Path.Combine(sourceRoot.FullName, typeConfig.SourcePath, includePath);
            var destPath = Path.Combine(config.Output.RootDir, typeConfig.OutputSubdir ?? "", includePath);

            if (Directory.Exists(sourcePath))
            {
                Console.WriteLine($"  Copying directory: {sourcePath} → {destPath}");
                CopyDirectory(sourcePath, destPath);
                copiedCount++;
            }
            else if (File.Exists(sourcePath))
            {
                Console.WriteLine($"  Copying file: {sourcePath} → {destPath}");
                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrWhiteSpace(destDir))
                    Directory.CreateDirectory(destDir);
                File.Copy(sourcePath, destPath, overwrite: true);
                copiedCount++;
            }
            else
            {
                Console.WriteLine($"  Warning: Include path not found: {sourcePath}");
            }
        }
    }

    if (copiedCount > 0)
    {
        Console.WriteLine($"Copied {copiedCount} include path(s).");
    }
    else
    {
        Console.WriteLine("No include paths to copy.");
    }
    Console.WriteLine();
}

void CopyDirectory(string sourceDir, string destDir)
{
    // Create destination directory
    Directory.CreateDirectory(destDir);

    // Copy all files
    foreach (var file in Directory.GetFiles(sourceDir))
    {
        var fileName = Path.GetFileName(file);
        var destFile = Path.Combine(destDir, fileName);
        File.Copy(file, destFile, overwrite: true);
    }

    // Recursively copy all subdirectories
    foreach (var subDir in Directory.GetDirectories(sourceDir))
    {
        var dirName = Path.GetFileName(subDir);
        var destSubDir = Path.Combine(destDir, dirName);
        CopyDirectory(subDir, destSubDir);
    }
}

// ===== Custom Handlebars File System =====

class CustomFileSystem : ViewEngineFileSystem
{
    private readonly string _templatesDir;

    public CustomFileSystem(string templatesDir)
    {
        _templatesDir = templatesDir;
    }

    protected override string CombinePath(string dir, string otherFileName)
    {
        return Path.Combine(_templatesDir, otherFileName);
    }

    public override bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(_templatesDir, filePath);
        return File.Exists(fullPath) || File.Exists(fullPath + ".hbs") || File.Exists(fullPath + ".html");
    }

    public override string GetFileContent(string filename)
    {
        var fullPath = Path.Combine(_templatesDir, filename);
        if (File.Exists(fullPath))
            return File.ReadAllText(fullPath);

        fullPath += ".hbs";
        if (File.Exists(fullPath))
            return File.ReadAllText(fullPath);

        fullPath = fullPath.Replace(".hbs", ".html");
        if (File.Exists(fullPath))
            return File.ReadAllText(fullPath);

        throw new FileNotFoundException($"Template not found: {filename}");
    }
}

// ===== Data Models =====

record Config
{
    public SourceConfig Source { get; init; } = new();
    public OutputConfig Output { get; init; } = new();
    public Dictionary<string, ContentTypeConfig> ContentTypes { get; init; } = new();
    public PathsConfig? Paths { get; init; }
    public List<string>? AccentColors { get; init; }
    public DefaultsConfig? Defaults { get; init; }
}

record SourceConfig
{
    public string RootDir { get; init; } = "docs";
    public string TemplatesDir { get; init; } = "templates";
    public List<string>? ExcludeDirs { get; init; }
    public List<string>? ExcludeFiles { get; init; }
}

record OutputConfig
{
    public string RootDir { get; init; } = "w";
    public bool PreserveStructure { get; init; } = true;
}

record ContentTypeConfig
{
    public string? SourcePath { get; init; }
    public string? Template { get; init; }
    public List<string> CssFiles { get; init; } = new();
    public string? BackLink { get; init; }
    public string? OutputSubdir { get; init; }
    public bool IsIndex { get; init; }
    public bool IsSoftwareList { get; init; }
    public List<string>? IncludePaths { get; init; }
}

record DefaultsConfig
{
    public string? FooterText { get; init; }
}

record PathsConfig
{
    public string? ImagesBase { get; init; }
}

record Frontmatter
{
    public string Title { get; set; } = "";
    public string? Excerpt { get; init; }
    public string? Date { get; init; }
    public List<string>? Tags { get; init; }
}

/// <summary>
/// Processes Jekyll-style gallery liquid tags and renders them as HTML
/// </summary>
public static class Gallery
{
    /// <summary>
    /// Processes all {% include gallery %} tags in the content and replaces them with rendered HTML
    /// </summary>
    /// <param name="content">The HTML/markdown content containing liquid tags</param>
    /// <param name="frontmatterData">Dictionary containing frontmatter data including gallery arrays</param>
    /// <returns>Content with gallery tags replaced by rendered HTML</returns>
    public static string ProcessGalleries(string content, Dictionary<string, object> frontmatterData)
    {
        // Pattern to match {% include gallery ... %}
        var pattern = @"{%\s*include\s+gallery\s*([^%]*?)%}";
        var regex = new Regex(pattern, RegexOptions.Multiline);

        return regex.Replace(content, match =>
        {
            var parameters = ParseLiquidParameters(match.Groups[1].Value);
            return RenderGallery(parameters, frontmatterData);
        });
    }

    /// <summary>
    /// Parses liquid include parameters like: caption="My caption" id="gallery2" layout="half"
    /// </summary>
    private static Dictionary<string, string> ParseLiquidParameters(string paramString)
    {
        var parameters = new Dictionary<string, string>();

        // Match key="value" or key='value' patterns
        var paramPattern = @"(\w+)\s*=\s*[""']([^""']*)[""']";
        var matches = Regex.Matches(paramString, paramPattern);

        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            parameters[key] = value;
        }

        return parameters;
    }

    /// <summary>
    /// Renders the gallery HTML based on the template logic from gallery.liquid
    /// </summary>
    private static string RenderGallery(Dictionary<string, string> parameters, Dictionary<string, object> frontmatterData)
    {
        // Determine which gallery to use (from id parameter or default "gallery")
        var galleryId = parameters.GetValueOrDefault("id", "gallery");

        // Get gallery array from frontmatter
        List<Dictionary<string, object>>? galleryItems = null;

        if (frontmatterData.TryGetValue(galleryId, out var galleryObj))
        {
            // Convert to list of dictionaries
            if (galleryObj is List<object> objList)
            {
                galleryItems = objList
                    .OfType<Dictionary<object, object>>()
                    .Select(dict => dict.ToDictionary(
                        kvp => kvp.Key.ToString() ?? "",
                        kvp => kvp.Value
                    ))
                    .ToList();
            }
        }

        if (galleryItems == null || galleryItems.Count == 0)
        {
            return $"<!-- Gallery '{galleryId}' not found or empty -->";
        }

        // Determine layout (from parameter or auto-detect based on size)
        string layout;
        if (parameters.ContainsKey("layout"))
        {
            layout = parameters["layout"];
        }
        else
        {
            layout = galleryItems.Count switch
            {
                2 => "half",
                >= 3 => "third",
                _ => ""
            };
        }

        // Get optional class and caption
        var customClass = parameters.GetValueOrDefault("class", "");
        var caption = parameters.GetValueOrDefault("caption", "");

        // Build HTML
        var html = new StringBuilder();
        var figureClasses = string.Join(" ", new[] { layout, customClass }.Where(s => !string.IsNullOrWhiteSpace(s)));

        html.AppendLine($"<figure class=\"{figureClasses}\">");

        // Render each image
        foreach (var img in galleryItems)
        {
            var imagePath = GetValue(img, "image_path");
            var url = GetValue(img, "url");
            var alt = GetValue(img, "alt");
            var title = GetValue(img, "title");

            if (string.IsNullOrWhiteSpace(imagePath))
                continue;

            // If URL is provided, wrap image in anchor tag
            if (!string.IsNullOrWhiteSpace(url))
            {
                html.Append($"    <a href=\"{EscapeHtml(url)}\"");

                if (!string.IsNullOrWhiteSpace(title))
                {
                    html.Append($" title=\"{EscapeHtml(title)}\"");
                }

                html.AppendLine(">");
                html.AppendLine($"      <img src=\"{EscapeHtml(imagePath)}\" alt=\"{EscapeHtml(alt)}\">");
                html.AppendLine("    </a>");
            }
            else
            {
                html.AppendLine($"    <img src=\"{EscapeHtml(imagePath)}\" alt=\"{EscapeHtml(alt)}\">");
            }
        }

        // Add caption if provided
        if (!string.IsNullOrWhiteSpace(caption))
        {
            html.AppendLine($"  <figcaption>{EscapeHtml(caption)}</figcaption>");
        }

        html.AppendLine("</figure>");

        return html.ToString();
    }

    /// <summary>
    /// Safely gets a string value from a dictionary
    /// </summary>
    private static string GetValue(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && value != null)
        {
            return value.ToString() ?? "";
        }
        return "";
    }

    /// <summary>
    /// Basic HTML escaping for attribute values
    /// </summary>
    private static string EscapeHtml(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
