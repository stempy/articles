# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a static site generator (SSG) that converts Markdown files into HTML. It's designed to generate a personal articles/portfolio site with support for multiple content types: articles, software lists, portfolio items, and an index page.

**Key Technology:** C# (.NET 10.0) with top-level statements, Handlebars templating, YamlDotNet for config parsing, and Markdig for Markdown conversion.

## Build and Run Commands

### Build the project
```bash
dotnet build ConvertMarkdown/ConvertMarkdown.csproj
```

### Run the converter
```bash
dotnet run --project ConvertMarkdown/ConvertMarkdown.csproj
```

### Watch mode (auto-rebuild on changes)
```bash
dotnet watch run --project ConvertMarkdown/ConvertMarkdown.csproj
```

### Publish
```bash
dotnet publish ConvertMarkdown/ConvertMarkdown.csproj
```

## Architecture

### Processing Pipeline

1. **Config Loading** (`convert_config_generic.yml`): Defines source/output directories, content type mappings, templates, and CSS paths
2. **File Discovery**: Recursively finds Markdown files in `docs/`, respecting exclude patterns
3. **Content Type Detection**: Determines content type based on:
   - File location (e.g., `docs/posts/` → posts content type)
   - Content structure (software list detection via table headers)
   - Filename (`index.md` → index content type)
4. **Frontmatter Parsing**: YAML frontmatter extracted (title, date, excerpt, custom fields like gallery)
5. **Content Processing**:
   - **Software lists**: Custom parser extracts era sections, software tables, traits
   - **Index pages**: Parses article listings with status, dates, links
   - **Standard articles**: Markdig converts Markdown → HTML, gallery liquid tags processed
6. **Template Rendering**: Handlebars templates in `templates/` render final HTML
7. **Output**: HTML files written to `w/` directory (gitignored), preserving source structure

### Plugin Architecture

The converter uses a **plugin-based architecture** where different content types are handled by specialized processors. This allows for easy extension without modifying core code.

**Core Components:**

**ProcessorRegistry** (`ConvertMarkdown/Processors/ProcessorRegistry.cs`)
- Manages all registered content processors
- Processors are sorted by priority (higher = evaluated first)
- Finds the first processor that can handle a given file

**IContentProcessor** (`ConvertMarkdown/Processors/IContentProcessor.cs`)
- Interface that all processors implement
- `CanProcess()`: Determines if processor can handle the file
- `Process()`: Processes content and returns template data
- `ConfigureMarkdownPipeline()`: Configures Markdig if needed
- `Priority`: Determines evaluation order (higher first)

**Built-in Processors:**

**IndexPageProcessor** (`ConvertMarkdown/Processors/IndexPageProcessor.cs`)
- Priority: 90
- Handles `index.md` files
- Parses article listings with status, dates, links
- Extracts H1/H2/H3 structure for navigation

**SoftwareListProcessor** (`ConvertMarkdown/Processors/SoftwareListProcessor.cs`)
- Priority: 100
- Detects files with `| Software |` and `| Years Active |` tables
- Parses era sections, software tables, traits
- Supports badge/color mappings (currently hardcoded, will be config-driven)

**StandardArticleProcessor** (`ConvertMarkdown/Processors/StandardArticleProcessor.cs`)
- Priority: 0 (lowest - acts as fallback)
- Handles standard Markdown articles
- Uses Markdig pipeline for conversion
- Always matches (fallback processor)

**Supporting Components:**

**ConvertMarkdown.cs** (`ConvertMarkdown/ConvertMarkdown.cs:1-160`)
- Main entry point with top-level statements
- Sets up ProcessorRegistry and registers built-in processors
- Main processing loop: finds processor, processes content, renders template

**Helpers.cs** (`ConvertMarkdown/Helpers.cs`)
- `LoadConfig()`: YAML config deserialization
- `ParseFrontmatter()`: Extracts YAML frontmatter from Markdown
- `DetermineContentType()`: Content type detection for template/CSS config
- `GetOutputPath()`: Calculates output file paths
- `RegisterHandlebarsHelpers()`: Custom Handlebars helpers (eq, if, inc)
- `CopyIncludedPaths()`: Copies static assets

**Gallery.cs** (`ConvertMarkdown/Gallery.cs`)
- Processes Jekyll-style `{% include gallery %}` liquid tags
- Parses frontmatter gallery arrays
- Renders responsive image galleries with configurable layouts (half, third)
- Must run BEFORE processor to prevent wrapping gallery HTML in `<p>` tags

**CustomFileSystem.cs** (`ConvertMarkdown/CustomFileSystem.cs`)
- Handlebars file system adapter for template loading

**Frontmatter.cs** (`ConvertMarkdown/Frontmatter.cs`)
- DTO for frontmatter fields (Title, Date, Excerpt, Template, Tags)

**Dtos/** (`ConvertMarkdown/Dtos/`)
- Config DTOs for YAML deserialization (Config, ContentTypeConfig, ProcessorConfigDto, etc.)

### Content Type System

The SSG supports multiple content types configured in `convert_config_generic.yml`:

- **index**: Main landing page with article listings
- **software_list**: Special format with software tables organized by eras
- **posts**: Standard articles with Markdown content
- **portfolio**: Portfolio items
- **default**: Fallback for uncategorized content

Each content type specifies:
- `source_path`: Directory pattern to match
- `template`: Handlebars template to use
- `css_files`: CSS files to include (paths relative to output HTML)
- `back_link`: Navigation back link
- `output_subdir`: Output directory within `w/`
- `include_paths`: Additional files/directories to copy

### Template System

Templates are Handlebars (`.hbs`) files in `templates/`:
- `index.hbs`: Landing page with article cards
- `article.hbs`: Standard article layout
- `software-list.hbs`: Software list with era sections
- `portfolio.html`: Portfolio item layout

Templates receive context objects with keys like:
- `title`, `css_files`, `footer_text`
- `body_content` (for articles)
- `era_sections`, `traits` (for software lists)
- `articles` (for index pages)
- `accent_color` (rotated per file from config)

### CSS Class Naming Convention

The project uses **generic, semantic CSS classes** (see `CSS_CLASS_NAMING.md`):

**Preferred Modern Classes:**
- `.card-grid`, `.card`, `.card-header`, `.card-title`, `.card-description`
- `.era-section`, `.era-header`, `.era-badge`, `.era-title`
- `.article-section`, `.article-grid`, `.article-card`

**Deprecated (but backward compatible):**
- `.software-grid` → use `.card-grid`
- `.software-card` → use `.card`
- `.software-name` → use `.card-title`

**Accent Colors (CSS Custom Properties):**
- `--accent-gold`, `--accent-purple`, `--accent-cyan`, `--accent-rose`, `--accent-orange`, `--accent-green`, `--accent-blue`

When modifying templates or adding new content types, use the modern generic class names.

## Directory Structure

```
.
├── ConvertMarkdown/           # C# SSG source code
│   ├── ConvertMarkdown.cs     # Main entry point
│   ├── Helpers.cs             # Core helper functions
│   ├── SoftwareHelpers.cs     # Software list parsing
│   ├── Gallery.cs             # Gallery liquid tag processor
│   ├── Frontmatter.cs         # Frontmatter DTO
│   ├── CustomFileSystem.cs    # Handlebars file system
│   └── Dtos/                  # Configuration DTOs
├── docs/                      # Source Markdown files
│   ├── index.md               # Main landing page
│   ├── posts/                 # Article markdown files
│   └── portfolio/             # Portfolio markdown files
├── templates/                 # Handlebars templates
│   ├── index.hbs
│   ├── article.hbs
│   ├── software-list.hbs
│   └── partials/
├── css/                       # CSS stylesheets
├── w/                         # Generated HTML output (gitignored)
├── convert_config_generic.yml # SSG configuration
└── CSS_CLASS_NAMING.md        # CSS convention documentation
```

## Important Implementation Details

### Gallery Processing Order
Gallery liquid tags MUST be processed BEFORE Markdig conversion (`ConvertMarkdown.cs:99`), otherwise the rendered HTML gets wrapped in `<p>` tags.

### Frontmatter Title Fallback
The system has multiple fallback strategies for missing titles:
1. YAML frontmatter `title` field
2. First H1 (`# Title`) in document body
3. Formatted filename (e.g., `my-article.md` → "My Article")

### Template Override via Frontmatter
Individual markdown files can override their content type's default template using the `template:` frontmatter field. Template selection priority (`ConvertMarkdown.cs:122-125`):
1. Frontmatter `template` field (e.g., `template: software-list`)
2. Content type's configured template
3. Default template (`default.hbs`)

Example:
```yaml
---
template: software-list
title: My Article
---
```

### Software List Detection
A file is detected as a software list if it contains both:
- `| Software |` header
- `| Years Active |` header

### Content Type Priority
Detection order: index.md → software list (content-based) → path-based content types → default

### Accent Color Rotation
Accent colors are assigned per file using modulo index (`Helpers.cs:142-146`):
```csharp
var accentColor = colors[index % colors.Count];
```

### Handlebars Custom Helpers
- `eq`: Equality comparison (`{{#if (eq status "live")}}`)
- `if`: Enhanced conditional with inverse support
- `inc`: Increment helper for 1-based indexing (`{{inc @index}}`)

## Configuration

The main config file `convert_config_generic.yml` controls:
- Source/output root directories
- Excluded directories/files
- Content type mappings
- Template assignments
- CSS file paths
- Accent color palette
- Default frontmatter values

When adding new content types or modifying the pipeline, update this file.

## Common Workflows

### Adding a New Content Type
1. Add entry to `content_types` in `convert_config_generic.yml`
2. Create corresponding Handlebars template in `templates/`
3. Add CSS file if needed in `css/`
4. Run converter to test

### Adding a New Article
1. Create `.md` file in `docs/posts/`
2. Add YAML frontmatter (title, date, excerpt)
3. Write Markdown content
4. Run converter: `dotnet run --project ConvertMarkdown/ConvertMarkdown.csproj`
5. Output appears in `w/posts/`

### Adding Custom Processors

To add a new content processor for a custom content type:

**1. Create a new processor class in `ConvertMarkdown/Processors/`:**

```csharp
public class TutorialProcessor : IContentProcessor
{
    public int Priority => 80; // Higher than standard (0) but lower than software list (100)
    public string ProcessorType => "tutorial";

    public bool CanProcess(FileInfo file, string content, ProcessorContext context)
    {
        // Detect by frontmatter type or content pattern
        return content.Contains("## Prerequisites") && content.Contains("## Steps");
    }

    public MarkdownPipelineBuilder? ConfigureMarkdownPipeline(MarkdownPipelineBuilder builder)
    {
        // Return null if you handle HTML generation yourself
        // Or configure Markdig pipeline for markdown conversion
        return builder
            .UseAdvancedExtensions()
            .UseTaskLists();
    }

    public ProcessorResult Process(string body, Dictionary<string, object> rawFrontmatter,
        ProcessorContext context)
    {
        // Parse your custom content format
        var sections = ParseTutorialSections(body);

        return new ProcessorResult
        {
            TemplateData = new Dictionary<string, object>
            {
                ["sections"] = sections,
                ["difficulty"] = DetermineDifficulty(body),
                ["estimated_time"] = CalculateTime(sections)
            }
        };
    }

    private List<Dictionary<string, object>> ParseTutorialSections(string body)
    {
        // Custom parsing logic here
        return new List<Dictionary<string, object>>();
    }
}
```

**2. Register the processor in `ConvertMarkdown.cs`:**

Find the processor registry setup (around line 30) and add:
```csharp
registry.Register(new TutorialProcessor());
```

**3. Create a corresponding template in `templates/`:**

Create `templates/tutorial.hbs` with your custom HTML structure.

**4. (Optional) Add content type config in `convert_config_generic.yml`:**

```yaml
content_types:
  tutorials:
    source_path: tutorials
    template: tutorial.html
    css_files:
      - ../../css/styles.css
      - ../../css/tutorial.css
    back_link: ../../index.html
    output_subdir: tutorials
```

**Priority Guidelines:**
- 100+: High-priority special formats (software lists, data tables)
- 50-99: Medium-priority custom content (tutorials, reviews, guides)
- 1-49: Low-priority specialized articles
- 0: Standard article fallback (always matches)

### Debugging Template Issues
1. Check console output for "Using template: X" message
2. Verify template exists in `templates/` directory
3. Check template data keys match what's passed from code
4. Use `Console.WriteLine` in helper functions to inspect data

### Adding Image Galleries
1. Add `gallery` array to frontmatter:
```yaml
gallery:
  - image_path: /images/photo1.jpg
    alt: Description
    title: Optional title
```
2. Use liquid tag in body: `{% include gallery %}`
3. Optional parameters: `{% include gallery id="gallery2" layout="half" caption="My Gallery" %}`
