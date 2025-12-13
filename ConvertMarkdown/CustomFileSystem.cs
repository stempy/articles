//#:property RuntimeIdentifier=linux-x64

using HandlebarsDotNet;
// ===== Custom Handlebars File System =====

class CustomFileSystem : ViewEngineFileSystem
{
    private readonly string _templatesDir;
    private readonly string _layoutsDir;

    public CustomFileSystem(string templatesDir)
    {
        _templatesDir = templatesDir;
        _layoutsDir = Path.Combine(templatesDir, "layouts");
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

    public void RegisterLayoutPartials(IHandlebars handlebars)
    {
        if (!Directory.Exists(_layoutsDir))
        {
            Console.WriteLine($"[Layouts] Directory not found: {_layoutsDir}");
            return;
        }

        var layoutFiles = Directory.GetFiles(_layoutsDir, "*.hbs");
        foreach (var layoutFile in layoutFiles)
        {
            var layoutName = Path.GetFileNameWithoutExtension(layoutFile);
            var layoutContent = File.ReadAllText(layoutFile);

            // Register as partial (Handlebars.Net handles {{!< layout}} automatically)
            handlebars.RegisterTemplate(layoutName, layoutContent);
        }

        Console.WriteLine($"[Layouts] Registered {layoutFiles.Length} layout partials");
    }

    public void RegisterIncludePartials(IHandlebars handlebars)
    {
        var partialsDir = Path.Combine(_templatesDir, "partials");

        if (!Directory.Exists(partialsDir))
        {
            Console.WriteLine($"[Partials] Directory not found: {partialsDir}");
            Console.WriteLine($"[Partials] Skipping partial registration (create {partialsDir} to use {{% include %}} tags)");
            return;
        }

        var partialFiles = Directory.GetFiles(partialsDir, "*.hbs");
        foreach (var partialFile in partialFiles)
        {
            var partialName = Path.GetFileNameWithoutExtension(partialFile);
            var partialContent = File.ReadAllText(partialFile);

            // Register as partial for use in {% include TEMPLATE data=KEY %} tags
            handlebars.RegisterTemplate(partialName, partialContent);
            Console.WriteLine($"[Partials] Registered partial: {partialName}");
        }

        Console.WriteLine($"[Partials] Registered {partialFiles.Length} include partials");
    }
}
