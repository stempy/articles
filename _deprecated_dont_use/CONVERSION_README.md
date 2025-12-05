# Generic Markdown to HTML Conversion System

A flexible and modular system for converting all markdown files under `/docs` to HTML while preserving directory structure and applying appropriate templates based on content type.

## Features

- **Recursive conversion**: Converts all `.md` files under `/docs` directory
- **Directory structure preservation**: Maintains the same folder structure in output
- **Multiple content types**: Different templates and styles for different content (portfolio, posts, default)
- **Separated templates**: Template files stored in `/templates` directory
- **Separated CSS**: CSS files stored in `/css` directory
- **Automatic title extraction**: Extracts title from YAML frontmatter or first H1 heading
- **Flexible configuration**: YAML-based configuration for easy customization

## Directory Structure

```
articles/
├── docs/                           # Source markdown files
│   ├── portfolio/                  # Portfolio items
│   ├── posts/                      # Blog posts/articles
│   └── ...
├── templates/                      # HTML templates
│   ├── index.html                 # Template for index pages (card-based)
│   ├── article.html               # Template for blog posts
│   ├── portfolio.html             # Template for portfolio items
│   └── default.html               # Default template
├── css/                           # Separated CSS files
│   ├── article.css                # Styles for articles
│   └── portfolio.css              # Styles for portfolio
├── w/                             # Output directory
│   ├── portfolio/                 # Generated portfolio HTML
│   ├── posts/                     # Generated post HTML
│   └── ...
├── convert_md_to_html.py          # Main conversion script
└── convert_config_generic.yml     # Configuration file
```

## Configuration

Edit `convert_config_generic.yml` to customize the conversion:

### Source Settings

```yaml
source:
  root_dir: docs                    # Root directory to scan
  templates_dir: templates          # Template directory
  exclude_dirs:                     # Directories to skip
    - _pages
    - templates
  exclude_files:                    # Files to skip
    - README.md
```

### Content Types

Define different content types with specific templates and CSS:

```yaml
content_types:
  index:                            # Special index page type
    source_path: ''                 # Match index.md in root
    template: index.html            # Use card-based template
    css_files:
      - ../css/styles.css
    back_link: ''                   # No back link for index
    output_subdir: ''               # Output to root of w/
    is_index: true                  # Enable article card parsing

  portfolio:
    source_path: portfolio          # Match docs/portfolio/
    template: portfolio.html        # Use this template
    css_files:                      # CSS files to include
      - ../../css/styles.css
      - ../../css/portfolio.css
    back_link: ../../index.html     # Navigation link
    output_subdir: portfolio        # Output to w/portfolio/

  posts:
    source_path: posts              # Match docs/posts/
    template: article.html
    css_files:
      - ../../css/styles.css
      - ../../css/article.css
    back_link: ../../index.html
    output_subdir: posts

  default:                          # Fallback for other files
    template: default.html
    css_files:
      - ../css/styles.css
    back_link: ../index.html
```

**Note:** The `is_index: true` flag enables special parsing for card-based layouts. Any content type can use this feature by setting this flag and structuring their markdown appropriately.

### Accent Colors

Rotating colors applied to each file:

```yaml
accent_colors:
  - gold
  - purple
  - cyan
  - rose
  - green
  - blue
  - orange
```

## Templates

Templates use placeholder variables that are replaced during conversion:

### Common Placeholders

- `{{title}}` - Document title
- `{{accent_color}}` - Rotating accent color
- `{{css_links}}` - Generated CSS link tags
- `{{back_link}}` - Navigation back link
- `{{date}}` - Formatted date
- `{{excerpt}}` - Document excerpt/description
- `{{body_content}}` - Converted markdown content
- `{{footer_text}}` - Footer text

### Index Template Placeholders (Card-Based Layout)

The index template (`index.html`) uses a card-based article grid structure:

- `{{subtitle}}` - Subtitle text
- `{{header_title}}` - Main header title (can include HTML like `<span>`)
- `{{intro}}` - Introduction paragraph
- `{{section_label}}` - Section label (e.g., "Collections")
- `{{section_title}}` - Section heading (e.g., "Published & Upcoming")
- `{{article_cards}}` - Generated article cards HTML

#### Article Card Markdown Structure

To use the index template, structure your markdown like this:

```markdown
# Stempy Editions

## Collections on Durable Software Development

Notes, research, and curated content

---

## Published & Upcoming

### Software That Stands the Test of Time
**Status:** Live | Updated November 2025

A tribute to long-running applications that remain actively developed years after release.

[Explore](long-lasting-software.html)

---

### Programming Language Comparisons
**Status:** In Progress | Updated October 2025

Comparing modern programming languages to find the best for your needs.

[Explore](programming-language-comparisons.html)
```

The script will automatically:
- Extract article titles from H3 headers
- Parse status and date from bold text
- Extract summaries from paragraphs
- Extract links from `[Explore](url)` format
- Generate properly styled article cards with rotating accent colors

### Portfolio-Specific Placeholders

- `{{meta_items}}` - Metadata items (role, period, etc.)
- `{{tech_tags}}` - Technology tags
- `{{hero_image}}` - Hero/teaser image
- `{{gallery}}` - Image gallery
- `{{formatted_date}}` - Formatted date string

## Usage

### Basic Conversion

Convert all markdown files under `/docs`:

```bash
python3 convert_md_to_html.py
```

### Output

```
Found 22 markdown files to convert
Source directory: docs
Output directory: w

Converting: portfolio/appbrowser.md
  Type: portfolio
  → w/portfolio/appbrowser.html

Converting: posts/long-lasting-software.md
  Type: posts
  → w/posts/long-lasting-software.html

Conversion complete! Converted 22 of 22 files.
```

## Markdown Frontmatter

### With YAML Frontmatter

```markdown
---
title: My Article Title
date: 2025-01-15
excerpt: A brief description
tags:
  - javascript
  - web
---

Article content here...
```

### Without Frontmatter

If no frontmatter is present, the script automatically extracts the title from the first H1 heading:

```markdown
# My Article Title

Article content here...
```

## Supported Markdown Features

- **Headers**: `#`, `##`, `###`, `####`
- **Bold**: `**text**`
- **Italic**: `*text*`
- **Links**: `[text](url)`
- **Images**: `![alt](path)`
- **Code**: `` `code` ``
- **Lists**: `- item` or `* item`
- **Tables**: Standard markdown tables
- **Horizontal rules**: `---`

## Adding New Content Types

1. Create a new template in `/templates`
2. (Optional) Create a new CSS file in `/css`
3. Add a new content type in `convert_config_generic.yml`:

```yaml
content_types:
  tutorials:
    source_path: tutorials
    template: tutorial.html
    css_files:
      - ../../styles.css
      - ../../css/tutorial.css
    back_link: ../../index.html
    output_subdir: tutorials
```

4. Run the conversion script

## Comparison with Original Script

### Old: `convert_portfolio.py`
- ❌ Portfolio-specific only
- ❌ Hardcoded paths
- ❌ Templates embedded in portfolio directory
- ❌ Manual frontmatter parsing
- ❌ Single content type

### New: `convert_md_to_html.py`
- ✅ Converts all content types
- ✅ Configurable via YAML
- ✅ Centralized template directory
- ✅ YAML-based frontmatter parsing
- ✅ Multiple content types support
- ✅ Automatic title extraction
- ✅ Directory structure preservation
- ✅ Modular and extensible

## Customization

### Custom Templates

Create a new template in `/templates/my-template.html`:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <title>{{title}}</title>
    {{css_links}}
</head>
<body>
    <h1>{{title}}</h1>
    <div class="content">
        {{body_content}}
    </div>
</body>
</html>
```

### Custom CSS

Create a new CSS file in `/css/my-styles.css` and reference it in the config:

```yaml
css_files:
  - ../../styles.css
  - ../../css/my-styles.css
```

## Troubleshooting

### Files not being converted

Check `exclude_dirs` and `exclude_files` in the configuration.

### Wrong template applied

Verify the `source_path` matches your directory structure in `content_types`.

### Missing title

Ensure your markdown has either:
- YAML frontmatter with `title:` field
- A first-level heading (`# Title`)

### Broken CSS links

Adjust the relative paths in `css_files` based on your output directory depth.

## Migration Guide

To migrate from `convert_portfolio.py`:

1. ✅ New templates created in `/templates`
2. ✅ New CSS files created in `/css`
3. ✅ New configuration created: `convert_config_generic.yml`
4. ✅ New conversion script: `convert_md_to_html.py`

You can continue using `convert_portfolio.py` for portfolio-only conversions, or switch to `convert_md_to_html.py` for all content types.
