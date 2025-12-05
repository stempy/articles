using System.Text.RegularExpressions;

/// <summary>
/// Helper functions specific to software list content processing
/// </summary>
static class SoftwareHelpers
{
    public static bool IsSoftwareList(string content) =>
        content.Contains("| Software |") && content.Contains("| Years Active |");

    public static string FormatSoftwareListTitle(string title)
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

    public static Dictionary<string, object> ParseSoftwareList(string body, Config config)
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

    public static string BuildDescriptionHtml(List<string> descLines)
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

    public static List<Dictionary<string, object>> ParseSoftwareTable(List<string> tableLines)
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
}
