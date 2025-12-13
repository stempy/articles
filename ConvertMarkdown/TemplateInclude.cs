//#:property RuntimeIdentifier=linux-x64

using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Spectre.Console;

/// <summary>
/// Processes generic template include liquid tags and renders them using Handlebars partials
/// Syntax: {% include TEMPLATE_NAME data=FRONTMATTER_KEY %}
/// </summary>
public static class TemplateInclude
{
    /// <summary>
    /// Processes all {% include TEMPLATE data=KEY %} tags in the content
    /// </summary>
    /// <param name="content">The content containing liquid tags</param>
    /// <param name="frontmatterData">Dictionary containing frontmatter data</param>
    /// <param name="handlebars">Handlebars instance with registered partials</param>
    /// <param name="sourceFile">Optional source file for error reporting</param>
    /// <returns>Content with include tags replaced by rendered HTML</returns>
    public static string ProcessIncludes(
        string content,
        Dictionary<string, object> frontmatterData,
        IHandlebars handlebars,
        FileInfo? sourceFile = null)
    {
        // Pattern: {% include TEMPLATE_NAME data=KEY %}
        // Negative lookahead (?!gallery\b) excludes "gallery" to preserve Gallery.cs behavior
        // Captures: 1=template_name, 2=parameters (data=key, etc.)
        var pattern = @"{%\s*include\s+(?!gallery\b)(\w+)\s*([^%]*?)%}";
        var regex = new Regex(pattern, RegexOptions.Multiline);

        return regex.Replace(content, match =>
        {
            var templateName = match.Groups[1].Value;
            var parameters = ParseLiquidParameters(match.Groups[2].Value);
            return RenderTemplate(templateName, parameters, frontmatterData, handlebars, sourceFile);
        });
    }

    /// <summary>
    /// Parses liquid include parameters like: data="key" class="featured"
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
    /// Extracts data from frontmatter using a key path (supports dot notation for nested data)
    /// </summary>
    /// <param name="frontmatter">The frontmatter dictionary</param>
    /// <param name="dataPath">Key path (e.g., "gallery" or "sidebar.metrics")</param>
    /// <returns>The extracted data or null if not found</returns>
    private static object? ExtractDataFromFrontmatter(
        Dictionary<string, object> frontmatter,
        string dataPath)
    {
        // Handle simple key: "gallery"
        if (!dataPath.Contains('.'))
        {
            return frontmatter.GetValueOrDefault(dataPath);
        }

        // Handle nested path: "sidebar.metrics"
        var parts = dataPath.Split('.');
        object? current = frontmatter;

        foreach (var part in parts)
        {
            if (current is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(part, out var value))
                    return null;
                current = value;
            }
            else if (current is Dictionary<object, object> objDict)
            {
                // Handle YamlDotNet's Dictionary<object, object>
                if (!objDict.TryGetValue(part, out var value))
                    return null;
                current = value;
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Renders a template with the provided data
    /// </summary>
    private static string RenderTemplate(
        string templateName,
        Dictionary<string, string> parameters,
        Dictionary<string, object> frontmatterData,
        IHandlebars handlebars,
        FileInfo? sourceFile)
    {
        var sourceFileStr = sourceFile != null ? $" when processing {sourceFile.Name}" : "";

        // Check if template is registered
        if (!handlebars.Configuration.RegisteredTemplates.ContainsKey(templateName))
        {
            AnsiConsole.MarkupLine(
                "[yellow]⚠ Warning:[/] Template '{0}' not found in partials{1}",
                templateName.EscapeMarkup(),
                sourceFileStr.EscapeMarkup()
            );
            return $"<!-- Template '{templateName}' not found in partials -->";
        }

        // Get data parameter
        if (!parameters.TryGetValue("data", out var dataKey) || string.IsNullOrWhiteSpace(dataKey))
        {
            AnsiConsole.MarkupLine(
                "[yellow]⚠ Warning:[/] No 'data' parameter provided for template '{0}'{1}",
                templateName.EscapeMarkup(),
                sourceFileStr.EscapeMarkup()
            );
            return $"<!-- No 'data' parameter provided for template '{templateName}' -->";
        }

        // Extract data from frontmatter
        var data = ExtractDataFromFrontmatter(frontmatterData, dataKey);
        if (data == null)
        {
            AnsiConsole.MarkupLine(
                "[yellow]⚠ Warning:[/] Data key '{0}' not found in frontmatter{1}",
                dataKey.EscapeMarkup(),
                sourceFileStr.EscapeMarkup()
            );
            return $"<!-- Data key '{dataKey}' not found in frontmatter -->";
        }

        // Render template
        try
        {
            // Get the compiled template and render it
            var template = handlebars.Configuration.RegisteredTemplates[templateName];

            // Create a StringWriter to capture the output
            using var writer = new StringWriter();
            template(writer, data);
            return writer.ToString();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                "[yellow]⚠ Warning:[/] Error rendering template '{0}': {1}{2}",
                templateName.EscapeMarkup(),
                ex.Message.EscapeMarkup(),
                sourceFileStr.EscapeMarkup()
            );
            return $"<!-- Error rendering template '{templateName}': {ex.Message} -->";
        }
    }
}
