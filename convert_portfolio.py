#!/usr/bin/env python3
"""
Convert portfolio markdown files to HTML with the Stempy design system
"""

import os
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

def format_filename_as_title(filename):
    """Convert a filename to a nicely formatted title"""
    # Remove extension
    name = Path(filename).stem

    # Replace common separators with spaces
    name = name.replace('-', ' ').replace('_', ' ')

    # Title case the result
    return name.title()


def parse_frontmatter(content):
    """Extract YAML frontmatter from markdown content with nested structure support"""
    if not content.startswith('---'):
        # No frontmatter, try to extract title from first H1
        frontmatter = {}
        lines = content.split('\n')
        new_lines = []
        title_extracted = False

        for line in lines:
            if line.startswith('# ') and not title_extracted:
                frontmatter['title'] = line[2:].strip()
                title_extracted = True
                # Skip this line in the body content
                continue
            new_lines.append(line)

        return frontmatter, '\n'.join(new_lines)

    parts = content.split('---', 2)
    if len(parts) < 3:
        return {}, content

    frontmatter_text = parts[1].strip()
    body = parts[2].strip()

    # Parse YAML-like frontmatter with nested support
    frontmatter = {}
    current_key = None
    current_nested_key = None
    current_dict = None
    current_list = None
    current_list_item = None

    for line in frontmatter_text.split('\n'):
        line_rstrip = line.rstrip()

        # Skip comments and empty lines
        if not line_rstrip or line_rstrip.strip().startswith('#'):
            continue

        # Detect indentation level
        indent = len(line) - len(line.lstrip())

        # List item at second level (  - )
        if line_rstrip.startswith('  - '):
            value = line_rstrip[4:].strip()

            # If this is a list item with a key:value
            if ':' in value and not value.startswith('http'):
                key_part, val_part = value.split(':', 1)
                key_part = key_part.strip()
                val_part = val_part.strip().strip('"').strip("'")

                # Start a new dict item in the list
                current_list_item = {key_part: val_part}
                if current_list is not None:
                    current_list.append(current_list_item)
            else:
                # Simple list item
                if current_list is not None:
                    current_list.append(value)
            continue

        # Nested key under list item (    key: value)
        if indent >= 4 and ':' in line_rstrip:
            key_part, val_part = line_rstrip.strip().split(':', 1)
            key_part = key_part.strip()
            val_part = val_part.strip().strip('"').strip("'")

            if current_list_item is not None:
                current_list_item[key_part] = val_part
            continue

        # Second-level key (  key: value)
        if indent >= 2 and ':' in line_rstrip and not line_rstrip.startswith('  - '):
            key_part, val_part = line_rstrip.strip().split(':', 1)
            key_part = key_part.strip()
            val_part = val_part.strip().strip('"').strip("'")

            if current_key and current_key in frontmatter:
                if not isinstance(frontmatter[current_key], dict):
                    frontmatter[current_key] = {}
                frontmatter[current_key][key_part] = val_part
            current_nested_key = key_part
            continue

        # Top-level key-value pairs (no indent)
        if ':' in line_rstrip and not line_rstrip.startswith(' '):
            key, value = line_rstrip.split(':', 1)
            key = key.strip()
            value = value.strip().strip('"').strip("'")

            if value:
                frontmatter[key] = value
            else:
                # Prepare for nested structure or list
                frontmatter[key] = []
                current_list = frontmatter[key]

            current_key = key
            current_list_item = None

    # If frontmatter exists but has no title, try to extract from first H1 in body
    if 'title' not in frontmatter or not frontmatter['title']:
        lines = body.split('\n')
        for line in lines:
            if line.startswith('# '):
                frontmatter['title'] = line[2:].strip()
                break

    return frontmatter, body

def fix_image_path(path, config):
    """Convert absolute image path to relative path"""
    if not path:
        return path

    # Remove leading slash if present
    if path.startswith('/'):
        path = path[1:]

    # Use configured images base path
    images_base = config['paths']['images_base']

    # Path in markdown already includes _pages/, just prepend base
    return f"{images_base}/{path}"

def extract_images_from_frontmatter(frontmatter, config):
    """Extract all image references from frontmatter"""
    images = {
        'teaser': None,
        'sidebar_image': None,
        'gallery': []
    }

    # Extract header teaser
    header = frontmatter.get('header', {})
    if isinstance(header, dict):
        teaser = header.get('teaser')
        images['teaser'] = fix_image_path(teaser, config) if teaser else None

    # Extract sidebar image
    sidebar = frontmatter.get('sidebar', [])
    if isinstance(sidebar, list):
        for item in sidebar:
            if isinstance(item, dict) and 'image' in item:
                images['sidebar_image'] = fix_image_path(item['image'], config)
                break

    # Extract gallery images
    gallery = frontmatter.get('gallery', [])
    if isinstance(gallery, list):
        for item in gallery:
            if isinstance(item, dict):
                img_path = item.get('image_path', '')
                url = item.get('url', img_path)

                img_data = {
                    'url': fix_image_path(url, config) if url else '',
                    'image_path': fix_image_path(img_path, config) if img_path else '',
                    'alt': item.get('alt', 'Gallery image')
                }
                if img_data['image_path']:
                    images['gallery'].append(img_data)

    return images

def markdown_to_html(text, config):
    """Basic markdown to HTML conversion with image support"""
    if not text:
        return ""

    # Remove Jekyll includes
    text = re.sub(r'\{%.*?%\}', '', text)

    # Images (before links, as they use similar syntax)
    def convert_image(match):
        alt = match.group(1)
        src = match.group(2)
        # Fix image path to be relative
        src = fix_image_path(src, config)
        return f'<img src="{src}" alt="{alt}" class="content-image">'
    text = re.sub(r'!\[(.*?)\]\((.*?)\)', convert_image, text)

    # Headers
    text = re.sub(r'^### (.*?)$', r'<h3>\1</h3>', text, flags=re.MULTILINE)
    text = re.sub(r'^## (.*?)$', r'<h2>\1</h2>', text, flags=re.MULTILINE)
    text = re.sub(r'^# (.*?)$', r'<h1>\1</h1>', text, flags=re.MULTILINE)

    # Bold
    text = re.sub(r'\*\*(.*?)\*\*', r'<strong>\1</strong>', text)

    # Links
    text = re.sub(r'\[(.*?)\]\((.*?)\)', r'<a href="\2">\1</a>', text)

    # Code blocks with backticks
    text = re.sub(r'`([^`]+)`', r'<code>\1</code>', text)

    # Horizontal rules
    text = re.sub(r'^---+$', '<hr>', text, flags=re.MULTILINE)

    # Lists
    lines = text.split('\n')
    html_lines = []
    in_list = False

    for line in lines:
        stripped = line.strip()

        # Unordered lists
        if stripped.startswith('- '):
            if not in_list:
                html_lines.append('<ul>')
                in_list = True
            html_lines.append(f'<li>{stripped[2:]}</li>')
        else:
            if in_list:
                html_lines.append('</ul>')
                in_list = False
            html_lines.append(line)

    if in_list:
        html_lines.append('</ul>')

    text = '\n'.join(html_lines)

    # Paragraphs (simple approach)
    paragraphs = text.split('\n\n')
    html_paragraphs = []

    for para in paragraphs:
        para = para.strip()
        if para:
            # Don't wrap if already has HTML tags
            if para.startswith('<'):
                html_paragraphs.append(para)
            else:
                # Replace single newlines with <br>
                para = para.replace('\n', '<br>\n')
                html_paragraphs.append(f'<p>{para}</p>')

    return '\n'.join(html_paragraphs)

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

def create_gallery_html(gallery_images):
    """Create HTML for image gallery"""
    if not gallery_images:
        return ''

    gallery_items = []
    for img in gallery_images:
        img_path = img['image_path']
        alt = img['alt']
        url = img.get('url', img_path)

        gallery_items.append(f'''
            <a href="{url}" class="gallery-item">
                <img src="{img_path}" alt="{alt}" loading="lazy">
            </a>
        ''')

    return f'''
        <section class="image-gallery">
            <h2>Gallery</h2>
            <div class="gallery-grid">
                {''.join(gallery_items)}
            </div>
        </section>
    '''

def load_template(template_name, config):
    """Load an HTML template file"""
    templates_dir = config['source']['templates_dir']
    template_path = Path(templates_dir) / template_name
    return template_path.read_text(encoding='utf-8')

def create_html(frontmatter, body_html, accent_color, images, config):
    """Create complete HTML page with images using template"""
    title = frontmatter.get('title', 'Portfolio Item')
    excerpt = frontmatter.get('excerpt', '')
    date = frontmatter.get('date', '')
    formatted_date = format_date(date)

    # Extract role and period from sidebar if available
    role = ""
    period = ""
    sidebar = frontmatter.get('sidebar', [])
    if isinstance(sidebar, list):
        for item in sidebar:
            if isinstance(item, dict):
                if item.get('title') == 'Role':
                    role = item.get('text', '')
                elif item.get('title') == 'Period':
                    period = item.get('text', '')

    # Build meta items
    meta_items = []
    if date:
        meta_items.append(f'<div class="portfolio-meta-item"><span class="portfolio-meta-label">Date:</span><span>{formatted_date}</span></div>')
    if role:
        meta_items.append(f'<div class="portfolio-meta-item"><span class="portfolio-meta-label">Role:</span><span>{role}</span></div>')
    if period:
        meta_items.append(f'<div class="portfolio-meta-item"><span class="portfolio-meta-label">Period:</span><span>{period}</span></div>')
    meta_items_html = '\n                '.join(meta_items)

    # Extract tags
    tags = frontmatter.get('tags', [])
    if isinstance(tags, list) and tags:
        tags_html = '<div class="tech-tags">' + ''.join([f'<span class="tech-tag">{tag}</span>' for tag in tags]) + '</div>'
    else:
        tags_html = ''

    # Create excerpt HTML
    excerpt_html = f'<p class="portfolio-excerpt">{excerpt}</p>' if excerpt else ''

    # Create teaser/hero image section
    hero_image_html = ''
    if images['teaser'] or images['sidebar_image']:
        img_src = images['teaser'] or images['sidebar_image']
        hero_image_html = f'''
            <div class="hero-image">
                <img src="{img_src}" alt="{title}">
            </div>
        '''

    # Create gallery
    gallery_html = create_gallery_html(images['gallery'])

    # Load template and replace placeholders
    template = load_template(config['templates']['portfolio_item'], config)
    html = template.replace('{{title}}', title)
    html = html.replace('{{accent_color}}', accent_color)
    html = html.replace('{{meta_items}}', meta_items_html)
    html = html.replace('{{excerpt}}', excerpt_html)
    html = html.replace('{{tech_tags}}', tags_html)
    html = html.replace('{{hero_image}}', hero_image_html)
    html = html.replace('{{body_content}}', body_html)
    html = html.replace('{{gallery}}', gallery_html)
    html = html.replace('{{formatted_date}}', formatted_date)

    return html

def main():
    """Main conversion function"""
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

    print(f"Found {len(md_files)} markdown files to convert")
    print(f"Output directory: {output_dir}")

    for index, md_file in enumerate(md_files):
        print(f"Converting: {md_file.name}")

        # Read markdown file
        content = md_file.read_text(encoding='utf-8')

        # Parse frontmatter and body
        frontmatter, body = parse_frontmatter(content)

        # Ensure there's always a title - fallback to formatted filename
        if 'title' not in frontmatter or not frontmatter['title']:
            frontmatter['title'] = format_filename_as_title(md_file.name)

        # Extract images from frontmatter
        images = extract_images_from_frontmatter(frontmatter, config)

        # Count images found
        img_count = sum([
            1 if images['teaser'] else 0,
            1 if images['sidebar_image'] else 0,
            len(images['gallery'])
        ])
        if img_count > 0:
            print(f"  Found {img_count} image(s)")

        # Convert markdown to HTML
        body_html = markdown_to_html(body, config)

        # Get accent color
        accent_color = get_accent_color(index)

        # Create HTML
        html = create_html(frontmatter, body_html, accent_color, images, config)

        # Write HTML file to output directory
        html_file = output_dir / f"{md_file.stem}.html"
        html_file.write_text(html, encoding='utf-8')

        print(f"  → Created: {html_file}")

    print(f"\nConversion complete! Created {len(md_files)} HTML files in {output_dir}/.")

if __name__ == '__main__':
    main()
