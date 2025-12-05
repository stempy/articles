//#:property RuntimeIdentifier=linux-x64

using HandlebarsDotNet;
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
