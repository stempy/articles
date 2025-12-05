//#:property RuntimeIdentifier=linux-x64

using System.Text.RegularExpressions;
using Markdig;

/// <summary>
/// Processor for index page content with article listings
/// </summary>
public class IndexPageProcessor : IContentProcessor
{
    public int Priority => 90; // High priority to match before standard articles

    public string ProcessorType => "index_page";

    public bool CanProcess(FileInfo file, string content, ProcessorContext context)
    {
        return file.Name == "index.md";
    }

    public MarkdownPipelineBuilder? ConfigureMarkdownPipeline(MarkdownPipelineBuilder builder)
    {
        return null; // We handle parsing ourselves
    }

    public ProcessorResult Process(string body, Dictionary<string, object> rawFrontmatter, ProcessorContext context)
    {
        // Get status map from config or use defaults
        var statusMap = GetStatusMap(context.ProcessorConfig);

        var indexData = ParseIndexContent(body, context.GlobalConfig, statusMap);

        return new ProcessorResult
        {
            TemplateData = indexData,
            SkipMarkdownConversion = true
        };
    }

    private Dictionary<string, string> GetStatusMap(Dictionary<string, object> config)
    {
        if (config.TryGetValue("status_map", out var value) && value is Dictionary<object, object> dict)
        {
            return dict.ToDictionary(
                kvp => kvp.Key.ToString() ?? "",
                kvp => kvp.Value?.ToString() ?? ""
            );
        }
        return new Dictionary<string, string>
        {
            ["Live"] = "live",
            ["In Progress"] = "soon",
            ["Soon"] = "soon",
            ["Upcoming"] = "soon"
        };
    }

    public static Dictionary<string, object> ParseIndexContent(string body, Config config, Dictionary<string, string> statusMap)
    {
        var lines = body.Split('\n');
        var title = "Stempy Editions";
        var subtitle = "Stempy Editions";
        var headerTitle = "Collections on <span>Durable Software Development</span>";
        var intro = "Notes, research, and curated content";
        var sectionLabel = "Collections";
        var sectionTitle = "Published & Upcoming";
        var articles = new List<Dictionary<string, object>>();

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
                    currentArticle["status_class"] = statusMap.GetValueOrDefault((string?)currentArticle.GetValueOrDefault("status") ?? "Live", "live");
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
            currentArticle["status_class"] = statusMap.GetValueOrDefault((string?)currentArticle.GetValueOrDefault("status") ?? "Live", "live");
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
}
