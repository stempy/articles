using System.Text;
using System.Text.RegularExpressions;

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
