#!/usr/bin/env python3
"""
Generate portfolio index.html with all portfolio items sorted by date
"""

import re
import yaml
from pathlib import Path
from datetime import datetime

def load_config(config_file='convert_config.yml'):
    """Load configuration from YAML file"""
    config_path = Path(config_file)
    if not config_path.exists():
        raise FileNotFoundError(f"Configuration file not found: {config_file}")

    with open(config_path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)

def parse_frontmatter(content):
    """Extract YAML frontmatter from markdown content"""
    if not content.startswith('---'):
        return {}, content

    parts = content.split('---', 2)
    if len(parts) < 3:
        return {}, content

    frontmatter_text = parts[1].strip()
    frontmatter = {}
    current_key = None
    current_list = None

    for line in frontmatter_text.split('\n'):
        line_rstrip = line.rstrip()

        # Skip comments and empty lines
        if not line_rstrip or line_rstrip.strip().startswith('#'):
            continue

        # Handle list items
        if line_rstrip.startswith('  - '):
            if current_list is not None:
                current_list.append(line_rstrip[4:].strip())
            continue

        # Top-level key-value pairs
        if ':' in line_rstrip and not line_rstrip.startswith(' '):
            key, value = line_rstrip.split(':', 1)
            key = key.strip()
            value = value.strip().strip('"').strip("'")

            if value:
                frontmatter[key] = value
            else:
                frontmatter[key] = []
                current_list = frontmatter[key]

            current_key = key

    return frontmatter, ""

def get_accent_color(index):
    """Get accent color based on index"""
    colors = ['gold', 'purple', 'cyan', 'rose', 'green', 'blue', 'orange']
    return colors[index % len(colors)]

def format_date(date_str):
    """Format date string"""
    try:
        date_obj = datetime.strptime(date_str, '%Y-%m-%d')
        return date_obj.strftime('%B %Y')
    except:
        return date_str

def parse_date_for_sorting(date_str):
    """Parse date for sorting purposes"""
    try:
        return datetime.strptime(date_str, '%Y-%m-%d')
    except:
        return datetime(1970, 1, 1)  # Default to epoch for invalid dates

def load_template(template_name, config):
    """Load an HTML template file"""
    templates_dir = config['source']['templates_dir']
    template_path = Path(templates_dir) / template_name
    return template_path.read_text(encoding='utf-8')

def create_portfolio_index():
    """Generate portfolio index.html"""
    # Load configuration
    config = load_config()

    portfolio_dir = Path(config['source']['portfolio_dir'])
    output_dir = Path(config['output']['html_dir'])

    if not portfolio_dir.exists():
        print(f"Error: {portfolio_dir} directory not found")
        return

    # Create output directory
    output_dir.mkdir(parents=True, exist_ok=True)

    # Get all markdown files
    md_files = sorted(portfolio_dir.glob('*.md'))

    # Parse all portfolio items
    portfolio_items = []

    for md_file in md_files:
        content = md_file.read_text(encoding='utf-8')
        frontmatter, _ = parse_frontmatter(content)

        item = {
            'filename': md_file.stem,
            'title': frontmatter.get('title', 'Untitled'),
            'date': frontmatter.get('date', ''),
            'excerpt': frontmatter.get('excerpt', ''),
            'tags': frontmatter.get('tags', []),
            'sort_date': parse_date_for_sorting(frontmatter.get('date', ''))
        }
        portfolio_items.append(item)

    # Sort by date descending (newest first)
    portfolio_items.sort(key=lambda x: x['sort_date'], reverse=True)

    # Load card template
    card_template = load_template(config['templates']['portfolio_card'], config)

    # Generate HTML cards
    cards_html = []
    for index, item in enumerate(portfolio_items):
        accent_color = get_accent_color(index)
        formatted_date = format_date(item['date']) if item['date'] else 'Date unknown'

        # Create tags HTML
        tags_html = ''
        if isinstance(item['tags'], list) and item['tags']:
            tags_preview = item['tags'][:3]  # Show first 3 tags
            tags_html = '<div class="card-tags">' + ''.join([
                f'<span class="tech-tag">{tag}</span>' for tag in tags_preview
            ]) + '</div>'

        # Replace placeholders in card template
        card_html = card_template.replace('{{accent_color}}', accent_color)
        card_html = card_html.replace('{{formatted_date}}', formatted_date)
        card_html = card_html.replace('{{title}}', item['title'])
        card_html = card_html.replace('{{excerpt}}', item['excerpt'])
        card_html = card_html.replace('{{tech_tags}}', tags_html)
        card_html = card_html.replace('{{filename}}', item['filename'])

        cards_html.append(card_html)

    # Load index template and replace placeholders
    template = load_template(config['templates']['portfolio_index'], config)
    html = template.replace('{{project_count}}', str(len(portfolio_items)))
    html = html.replace('{{portfolio_cards}}', '\n                    '.join(cards_html))

    # Write index.html to output directory
    index_path = output_dir / 'index.html'
    index_path.write_text(html, encoding='utf-8')

    print(f"✓ Generated {index_path} with {len(portfolio_items)} items")
    print(f"  Sorted by date (newest first)")

if __name__ == '__main__':
    create_portfolio_index()
