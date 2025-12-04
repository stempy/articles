# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a static site generator (SSG) for converting markdown files to HTML. The primary implementation is a single-file C# script (`ConvertMarkdown.cs`) using .NET 10's `#package` directive syntax, which processes markdown files from `docs/` and outputs HTML to `w/`.

**Branch Strategy:**
- Main branch: `main`

## Build & Run Commands

### Convert Markdown to HTML

```bash
# Run the C# converter (primary method)
dotnet run ConvertMarkdown.cs

# First run downloads NuGet packages automatically
# Output: Converts all markdown files from docs/ to HTML in w/
```

### Preview Generated Site

The generated HTML files are in the `w/` directory. Open them directly in a browser or serve with a local HTTP server:

```bash
# Using .NET
dotnet tool install --global dotnet-serve
dotnet serve -d w -p 8000
```

## Project Architecture

### Core Components

1. **ConvertMarkdown.cs** (930 lines)
   - Single-file .NET 10 script with inline NuGet package references
   - Uses Handlebars.Net templating engine
   - Markdig for markdown conversion (CommonMark + extensions)
   - YamlDotNet for frontmatter parsing
   - Supports three content types: standard articles, software lists, and index pages

2. **Content Types Detection**
   The converter automatically detects content type based on:
   - **Index pages:** Filename is `index.md`
   - **Default:** Fallback for other content

3. **Template System** (Handlebars)
   - Templates in `templates/` with `.hbs` or `.html` extension
   - Uses Handlebars syntax: `{{var}}`, `{{{unescaped}}}`, `{{#each}}`, `{{#if}}`
   - Custom helpers registered: `eq`, `if`, `inc` (increment for 1-based indexing)
   - File system integration for partials support

4. **Configuration** (`convert_config_generic.yml`)
   - Defines content types, templates, CSS files per type
   - Source/output directory mapping
   - Exclude patterns for directories and files
   - Accent color rotation for styling

### Directory Structure

```
docs/                   # Source markdown files
├── index.md           # Site index (special: card-based article listing)
├── portfolio/         # Portfolio items
└── posts/             # Blog posts and software lists
templates/             # Handlebars templates
├── index.hbs          # Index page template
├── software-list.hbs  # Software list template
├── article.hbs        # Standard article template
└── partials/          # Reusable template fragments
css/                   # Stylesheets
└── styles.css         # Main stylesheet
w/                     # Generated HTML output (mirrors docs/ structure, and should be specified via convert_config_generic.yml)
```

### Markdown Frontmatter

Standard frontmatter (YAML):
```yaml
---
title: Article Title
date: 2025-01-15
excerpt: Brief description
tags:
  - tag1
  - tag2
---
```

**Title Extraction** (3-tier fallback):
1. YAML frontmatter `title:` field
2. First H1 heading (`# Title`) in markdown body
3. Formatted filename (`my-article.md` → "My Article")

### CSS Class Naming Convention

The CSS uses **generic, semantic class names** (see CSS_CLASS_NAMING.md):

**Modern (preferred):**
- `.card-grid`, `.card`, `.card-title`, `.card-header`, `.card-meta`
- `.era-section`, `.era-header`, `.era-badge`, `.era-title`
- `.article-grid`, `.article-card`, `.article-status`

**Legacy (deprecated but supported):**
- `.software-grid` → Use `.card-grid`
- `.software-card` → Use `.card`
- `.software-name` → Use `.card-title`

## Template Variables

### Common Variables (all templates)
- `{{title}}` - Document title
- `{{css_files}}` - Array of CSS file paths
- `{{footer_text}}` - Footer text

### Standard Article Template
- `{{accent_color}}` - Rotating color (gold, purple, cyan, etc.)
- `{{back_link}}` - Navigation back link
- `{{date}}` - Formatted date (MMMM yyyy)
- `{{excerpt}}` - Document excerpt
- `{{{body_content}}}` - Converted markdown (unescaped HTML)

### Software List Template
- `{{subtitle}}` - Subtitle text
- `{{{header_title}}}` - Main header with HTML formatting
- `{{intro}}` - Introduction paragraph
- `{{era_sections}}` - Array of era objects:
  - `{{years}}` - Era years display
  - `{{name}}` - Era name
  - `{{badge_class}}` - CSS class for badge
  - `{{color}}` - Accent color for section
  - `{{{description}}}` - Section description HTML
  - `{{software}}` - Array of software items:
    - `{{name}}`, `{{year}}`, `{{years_active}}`, `{{category}}`, `{{description}}`, `{{url}}`
- `{{traits}}` - Array of trait objects:
  - `{{title}}`, `{{description}}`

## Development Workflow

### Making Changes

1. **Edit markdown** in `docs/` directory
2. **Run converter:** `dotnet run ConvertMarkdown.cs`
3. **Preview:** Open `w/index.html` in browser or use local server
4. **Modify templates** in `templates/` (Handlebars syntax)
5. **Update styles** in `css/`

### Adding New Content Types

1. Create template in `templates/new-type.hbs`
2. (Optional) Create CSS in `css/new-type.css`
3. Add content type to `convert_config_generic.yml`:
```yaml
content_types:
  new_type:
    source_path: new_type        # Matches docs/new_type/
    template: new-type.hbs
    css_files:
      - ../../css/styles.css
      - ../../css/new-type.css
    back_link: ../../index.html
    output_subdir: new_type      # Outputs to w/new_type/
```
4. Run converter

### Extending the Converter

**Add custom Handlebars helper** (ConvertMarkdown.cs:804-837):
```csharp
handlebars.RegisterHelper("uppercase", (context, arguments) =>
{
    if (arguments.Length > 0)
        return arguments[0].ToString()?.ToUpper() ?? "";
    return "";
});
```

**Add markdown extensions** (ConvertMarkdown.cs:98-104):
```csharp
var pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .UseAutoLinks()
    .UseBootstrap()
    .UseEmojiAndSmiley()
    .UseTaskLists()
    .UseDiagrams()           // Add this
    .UseMathematics()        // Or this
    .Build();
```

## File Exclusions

Configured in `convert_config_generic.yml`:
- **Excluded directories:** `_pages`, `templates`
- **Excluded files:** `README.md` (when not at root)

Deprecated Python scripts moved to `_deprecated_dont_use/`.

## Performance Notes

- C# compiled performance: ~0.8s for 100 files (vs ~2.5s Python)
- First run downloads NuGet packages (cached thereafter)
- Markdig provides robust CommonMark + extensions (vs regex in Python version)

## Git Workflow

- Deleted files visible in status are deprecated Python scripts and old CLAUDE.md
- Working on feature branch: `feature/ssg`
- Merge to `main` when ready for deployment
