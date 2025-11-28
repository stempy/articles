# CSS Class Naming Convention

## Overview

The CSS has been refactored to use **generic, semantic class names** that work for any type of article or content, not just software lists.

## Generic Classes (Preferred)

Use these modern, semantic class names for new content:

### Layout
- `.card-grid` - Grid layout for any type of cards
- `.card` - Generic card component for articles, items, etc.

### Card Structure
- `.card-header` - Card header with title and badges
- `.card-title` - Main title within a card (works for any content type)
- `.card-meta` - Metadata section (dates, authors, etc.)
- `.card-category` - Category badge/label
- `.card-description` - Card description text

### Sections
- `.era-section` - Section grouping related content
- `.era-header` - Section header
- `.era-badge` - Badge showing era/category (legends, veterans, etc.)
- `.era-title` - Section title
- `.section-description` - Descriptive text between section header and content

### Articles (Index Page)
- `.article-section` - Container for article listings
- `.article-grid` - Grid for article cards (extends `.card-grid`)
- `.article-card` - Article-specific card styling (extends `.card`)
- `.article-meta` - Article metadata (status, date)
- `.article-status` - Status badge (live, soon, etc.)
- `.article-title` - Article title
- `.article-summary` - Article summary text
- `.article-link` - Article link button

## Legacy Classes (Backward Compatible)

These classes still work but are **deprecated** in favor of generic names:

- `.software-grid` → Use `.card-grid`
- `.software-card` → Use `.card`
- `.software-name` → Use `.card-title`

The legacy classes are maintained for backward compatibility with existing HTML files.

## Examples

### Before (Software-specific)
```html
<div class="software-grid">
    <article class="software-card">
        <h3 class="software-name">Item Name</h3>
        <p class="card-description">Description</p>
    </article>
</div>
```

### After (Generic)
```html
<div class="card-grid">
    <article class="card">
        <h3 class="card-title">Item Name</h3>
        <p class="card-description">Description</p>
    </article>
</div>
```

## Benefits

1. **Semantic** - Classes describe what they are, not what specific content type they hold
2. **Reusable** - Same classes work for software lists, tools, articles, resources, etc.
3. **Flexible** - Easy to add new content types without creating new CSS
4. **Maintainable** - Clearer naming makes the code easier to understand
5. **Future-proof** - Generic names don't need to change as content evolves

## CSS Custom Properties

Cards use CSS custom properties for theming:

```css
.card {
    /* Each card can have its own accent color */
    --card-accent: var(--accent-gold);
}
```

Available accent colors:
- `--accent-gold` (default)
- `--accent-purple`
- `--accent-cyan`
- `--accent-rose`
- `--accent-orange`
- `--accent-green`
- `--accent-blue`

## Usage in Templates

### Jinja2
```jinja2
<div class="card-grid">
    {% for item in items %}
    <article class="card" style="--card-accent: var(--accent-{{ item.color }});">
        <div class="card-header">
            <h3 class="card-title">{{ item.name }}</h3>
        </div>
        <p class="card-description">{{ item.description }}</p>
    </article>
    {% endfor %}
</div>
```

## Migration Guide

To update existing HTML files:

1. Replace `software-grid` with `card-grid`
2. Replace `software-card` with `card`
3. Replace `software-name` with `card-title`

Or simply regenerate HTML files using the updated Jinja2 templates:
```bash
python3 convert_md_to_html_jinja2.py
```
