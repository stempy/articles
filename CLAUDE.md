# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a static website repository hosting curated collections about durable software development. The site features articles presented as interactive HTML pages with custom styling, focusing on long-lasting software and programming language comparisons.

## Architecture

### Content Structure

The repository follows a dual-format content approach:
- **Markdown sources** (`.md`) - Lightweight content for version control and easy editing
- **HTML pages** (`.html`) - Fully rendered pages with embedded styling and interactive elements
- **Shared styles** (`styles.css`) - Centralized CSS with custom properties for theming

### Key Files

- `index.html` - Landing page listing all article collections
- `styles.css` - Shared stylesheet with CSS custom properties

### Design System

The site uses a cohesive design language:
- **Color scheme**: Dark theme with accent colors (gold, purple, cyan, rose, green, blue)
- **Typography**:
  - Headlines: 'Instrument Serif' (serif font for titles)
  - Body: 'Space Grotesk' (sans-serif for readability)
  - Code/Meta: 'IBM Plex Mono' (monospace for technical elements)
- Use index.html as a design guide example

## Content Workflow

When adding or updating content:

1. **Markdown files** are the source of truth for article content
2. **HTML files** should be regenerated/updated when markdown changes
3. Keep consistency between `.md` and `.html` versions
4. Maintain the card-based structure in HTML with appropriate accent colors

## CSS Architecture

The stylesheet uses CSS custom properties for theming:
- Define colors in `:root` for easy theme-wide changes
- Each article card uses `style="--card-accent: var(--accent-colorname);"` for per-card theming
- Era badges use predefined classes (`.legends`, `.veterans`, `.established`, etc.)

## Deployment

The site is hosted via GitHub Pages (indicated by `CNAME` file). Any changes pushed to `main` branch will be automatically deployed.
