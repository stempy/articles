//#:property RuntimeIdentifier=linux-x64

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig;
using HandlebarsDotNet;

/// <summary>
/// Helper functions for the static site generator
/// </summary>
static class Helpers
{
    public static Config LoadConfig(string configFile)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var yaml = File.ReadAllText(configFile);
        return deserializer.Deserialize<Config>(yaml);
    }

    public static (Frontmatter frontmatter, string body, Dictionary<string, object> rawFrontmatter) ParseFrontmatter(string content)
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

    public static string FormatFilenameAsTitle(string filename)
    {
        var name = Path.GetFileNameWithoutExtension(filename);
        name = name.Replace('-', ' ').Replace('_', ' ');
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
    }

    public static (string type, ContentTypeConfig config) DetermineContentType(FileInfo file, Config config, string content)
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
        if (SoftwareHelpers.IsSoftwareList(content))
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

    public static string GetAccentColor(int index, Config config)
    {
        var colors = config.AccentColors ?? new List<string> { "gold", "purple", "cyan" };
        return colors[index % colors.Count];
    }

    public static string FormatDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return "";

        if (DateTime.TryParse(dateStr, out var date))
            return date.ToString("MMMM yyyy");

        return dateStr;
    }

    public static Dictionary<string, object> ParseIndexContent(string body, Config config)
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

    public static List<FileInfo> FindMarkdownFiles(DirectoryInfo root, Config config)
    {
        var files = new List<FileInfo>();

        foreach (var file in root.EnumerateFiles("*.md", SearchOption.AllDirectories))
        {
            if (!Helpers.ShouldExclude(file, root, config))
                files.Add(file);
        }

        return files.OrderBy(f => f.FullName).ToList();
    }

    public static bool ShouldExclude(FileInfo file, DirectoryInfo root, Config config)
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

    public static FileInfo GetOutputPath(FileInfo sourceFile, DirectoryInfo sourceRoot, Config config, ContentTypeConfig typeConfig)
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

    public static Func<object, string> LoadHandlebarsTemplate(IHandlebars handlebars, string templateName, Config config)
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

    public static void RegisterHandlebarsHelpers(IHandlebars handlebars)
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

    public static void CopyIncludedPaths(Config config, DirectoryInfo sourceRoot)
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
                    Helpers.CopyDirectory(sourcePath, destPath);
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

    public static void CopyDirectory(string sourceDir, string destDir)
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
            Helpers.CopyDirectory(subDir, destSubDir);
        }
    }
}
