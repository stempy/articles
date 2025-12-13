# Handlebars Partials for Template Includes

This directory contains reusable Handlebars templates that can be included in markdown files via liquid tags.

## Usage

**In markdown:**
```markdown
---
my_data:
  title: Example
  description: Example description
---

{% include TEMPLATE_NAME data="my_data" %}
```

**Important:** Values must be quoted (`data="my_data"`), not unquoted (`data=my_data`).

**Template naming:**
- Filename: `template_name.hbs`
- Include syntax: `{% include template_name data="KEY" %}`
- Must use lowercase, underscores for multi-word names

## Available Partials

### card.hbs

Display a card component with image, title, description, tags, and link.

**Expected data structure:**
```yaml
card_data:
  image: /path/to/image.jpg   # Optional
  title: Card Title
  description: Card description
  tags: [tag1, tag2]           # Optional
  link: https://example.com    # Optional
```

**Usage:**
```markdown
{% include card data="card_data" %}
```

### stats.hbs

Display a statistics grid with metric values and labels.

**Expected data structure:**
```yaml
metrics:
  - value: "10K+"
    label: Downloads
  - value: "500+"
    label: Stars
```

**Usage:**
```markdown
{% include stats data="metrics" %}
```

## Nested Data Access

You can access nested frontmatter data using dot notation:

```yaml
---
sidebar:
  metrics:
    - value: 100
      label: Downloads
---

{% include stats data="sidebar.metrics" %}
```

## Creating New Partials

1. Create `.hbs` file in this directory
2. Use Handlebars syntax: `{{variable}}`, `{{#if}}`, `{{#each}}`
3. Access Handlebars helpers: `eq`, `if`, `inc`
4. Test by including in a markdown file

**Important:** Do not indent the HTML in partial templates. Indented content will be treated as code blocks by the Markdown processor.

**Example:**

```handlebars
<div class="my-component">
{{#if title}}
<h3>{{title}}</h3>
{{/if}}
{{#each items}}
<p>{{this}}</p>
{{/each}}
</div>
```

## Error Handling

If a template or data key is not found, the system will:
1. Print a warning to the console
2. Insert an HTML comment in the output (e.g., `<!-- Template 'foo' not found in partials -->`)
3. Continue building (non-fatal)

**Example errors:**
- Missing template: `<!-- Template 'missing' not found in partials -->`
- Missing data key: `<!-- Data key 'missing_key' not found in frontmatter -->`
- No data parameter: `<!-- No 'data' parameter provided for template 'card' -->`

## Technical Notes

- Partials are pre-registered at build time (see `CustomFileSystem.RegisterIncludePartials`)
- Templates have access to all registered Handlebars helpers
- Data is extracted from frontmatter using dot notation (e.g., `sidebar.metrics`)
- The template include system does not affect the existing `{% include gallery %}` system
- Partials are processed BEFORE markdown conversion to prevent HTML from being wrapped in `<p>` tags
