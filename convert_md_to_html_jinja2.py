#!/usr/bin/env python3
"""
Generic Markdown to HTML Converter with Jinja2 Template System
Converts all markdown files under a directory tree to HTML while preserving structure
"""

import os
import re
import yaml
from pathlib import Path
from datetime import datetime
from jinja2 import Environment, FileSystemLoader, select_autoescape


def load_config(config_file='convert_config_generic.yml'):
    """Load configuration from YAML file"""
    config_path = Path(config_file)
    if not config_path.exists():
        raise FileNotFoundError(f"Configuration file not found: {config_file}")

    with open(config_path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)


def setup_jinja_env(config):
    """Set up Jinja2 environment with template directory"""
    templates_dir = config['source']['templates_dir']
    env = Environment(
        loader=FileSystemLoader(templates_dir),
        autoescape=select_autoescape(['html', 'xml']),
        trim_blocks=True,
        lstrip_blocks=True
    )
    return env


def parse_frontmatter(content):
    """Extract YAML frontmatter from markdown content"""
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

    try:
        frontmatter = yaml.safe_load(frontmatter_text)
        if frontmatter is None:
            frontmatter = {}
    except yaml.YAMLError:
        # Fallback to simple parsing if YAML fails
        frontmatter = {}
        for line in frontmatter_text.split('\n'):
            if ':' in line and not line.strip().startswith('#'):
                key, value = line.split(':', 1)
                frontmatter[key.strip()] = value.strip().strip('"').strip("'")

    # If frontmatter exists but has no title, try to extract from first H1 in body
    if 'title' not in frontmatter or not frontmatter['title']:
        lines = body.split('\n')
        for line in lines:
            if line.startswith('# '):
                frontmatter['title'] = line[2:].strip()
                break

    return frontmatter, body


def format_filename_as_title(filename):
    """Convert a filename to a nicely formatted title"""
    # Remove extension
    name = Path(filename).stem

    # Replace common separators with spaces
    name = name.replace('-', ' ').replace('_', ' ')

    # Title case the result
    return name.title()


def markdown_to_html(text, config, rel_depth=2):
    """Convert markdown to HTML"""
    if not text:
        return ""

    # Remove Jekyll includes
    text = re.sub(r'\{%.*?%\}', '', text)

    # Images (before links, as they use similar syntax)
    def convert_image(match):
        alt = match.group(1)
        src = match.group(2)
        # Fix image path if needed
        if src.startswith('/'):
            src = '../' * rel_depth + src[1:]
        return f'<img src="{src}" alt="{alt}" class="content-image">'
    text = re.sub(r'!\[(.*?)\]\((.*?)\)', convert_image, text)

    # Headers
    text = re.sub(r'^#### (.*?)$', r'<h4>\1</h4>', text, flags=re.MULTILINE)
    text = re.sub(r'^### (.*?)$', r'<h3>\1</h3>', text, flags=re.MULTILINE)
    text = re.sub(r'^## (.*?)$', r'<h2>\1</h2>', text, flags=re.MULTILINE)
    text = re.sub(r'^# (.*?)$', r'<h1>\1</h1>', text, flags=re.MULTILINE)

    # Bold and italic
    text = re.sub(r'\*\*\*(.*?)\*\*\*', r'<strong><em>\1</em></strong>', text)
    text = re.sub(r'\*\*(.*?)\*\*', r'<strong>\1</strong>', text)
    text = re.sub(r'\*(.*?)\*', r'<em>\1</em>', text)

    # Links
    text = re.sub(r'\[(.*?)\]\((.*?)\)', r'<a href="\2">\1</a>', text)

    # Code blocks with backticks
    text = re.sub(r'`([^`]+)`', r'<code>\1</code>', text)

    # Horizontal rules
    text = re.sub(r'^---+$', '<hr>', text, flags=re.MULTILINE)

    # Lists and paragraphs
    lines = text.split('\n')
    html_lines = []
    in_list = False

    for line in lines:
        stripped = line.strip()

        # Unordered lists
        if stripped.startswith('- ') or stripped.startswith('* '):
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


def is_software_list(content):
    """Check if markdown content is a software list with tables"""
    # Look for table headers that indicate a software list
    return '| Software |' in content and '| Years Active |' in content


def determine_content_type(file_path, config, content=''):
    """Determine content type based on file path and content"""
    content_types = config.get('content_types', {})

    # Get relative path from source root
    source_root = Path(config['source']['root_dir'])
    try:
        rel_path = Path(file_path).relative_to(source_root)
    except ValueError:
        return 'default', content_types.get('default', {})

    # Check if it's index.md (special case)
    if rel_path.name == 'index.md':
        index_config = content_types.get('index')
        if index_config:
            return 'index', index_config

    # Check if it's a software list based on content
    if content and is_software_list(content):
        software_list_config = content_types.get('software_list')
        if software_list_config:
            return 'software_list', software_list_config

    # Check each content type
    for type_name, type_config in content_types.items():
        if type_name in ['default', 'index', 'software_list']:
            continue
        source_path = type_config.get('source_path', '')
        if source_path and str(rel_path).startswith(source_path):
            return type_name, type_config

    # Return default if no match
    return 'default', content_types.get('default', {})


def format_date(date_str):
    """Format date string"""
    if not date_str:
        return ''
    try:
        date_obj = datetime.strptime(str(date_str), '%Y-%m-%d')
        return date_obj.strftime('%B %Y')
    except:
        return str(date_str)


def get_accent_color(index, config):
    """Get accent color based on index"""
    colors = config.get('accent_colors', ['gold', 'purple', 'cyan'])
    return colors[index % len(colors)]


def parse_software_list(body, config):
    """Parse software list markdown into structured data"""
    lines = body.split('\n')

    # Extract title and intro
    title = ''
    subtitle = 'A Curated Collection'
    intro = ''
    era_sections = []
    current_era = None
    current_table = []
    in_table = False
    traits = []
    in_traits = False
    section_description = []

    # Map era titles to badge classes
    badge_map = {
        'Legends': 'legends',
        'Veterans': 'veterans',
        'Established': 'established',
        'Mature': 'mature',
        'Proven': 'proven',
        'Rising Stars': 'rising',
        'Rising': 'rising',
        # Custom badge names
        'HOST': 'legends',
        'VIRTUALIZATION': 'veterans',
        'BROWSERS': 'established',
        '.NET STACK': 'mature',
        'TOOLS': 'proven',
        'AI': 'rising'
    }

    # Map era titles to accent colors (cycling through them)
    color_map = {
        'Legends': 'gold',
        'Veterans': 'purple',
        'Established': 'cyan',
        'Mature': 'blue',
        'Proven': 'rose',
        'Rising Stars': 'green',
        'Rising': 'green',
        # Custom badge names
        'HOST': 'gold',
        'VIRTUALIZATION': 'purple',
        'BROWSERS': 'cyan',
        '.NET STACK': 'blue',
        'TOOLS': 'rose',
        'AI': 'green'
    }

    i = 0
    while i < len(lines):
        line = lines[i].strip()

        # Extract main title (H1)
        if line.startswith('# '):
            title = line[2:].strip()
            # For "Software That Stands the Test of Time", add line break and span
            if 'Test of Time' in title:
                title = 'Software That Stands<br>the <span>Test of Time</span>'
            elif 'Tool Setup' in title:
                title = 'My Local <span>Tool Setup</span>'
            else:
                # Generic: add span around last 2-3 words
                parts = title.rsplit(' ', 2)
                if len(parts) > 1:
                    title = ' '.join(parts[:-2]) + ' <span>' + ' '.join(parts[-2:]) + '</span>'

        # Extract intro (first paragraph after title)
        elif line and not line.startswith('#') and not line.startswith('---') and not line.startswith('|') and not intro and not in_traits and not current_era:
            intro = line

        # Extract era header (H2 with parentheses)
        elif line.startswith('## ') and '(' in line:
            # Save previous era if exists
            if current_era and current_table:
                current_era['software'] = parse_software_table(current_table)
                current_era['description'] = build_description_html(section_description)
                era_sections.append(current_era)
                current_table = []

            # Reset section description for new section
            section_description = []

            # Parse era header - supports two formats:
            # 1. "30+ Years (Legends)" - traditional format
            # 2. "HOST (Windows 11 Host)" - badge-first format
            header_text = line[3:].strip()
            parts = header_text.split('(')
            first_part = parts[0].strip()
            second_part = parts[1].rstrip(')').strip() if len(parts) > 1 else first_part

            # Determine which format we have
            if first_part.isupper() or first_part in badge_map:
                # Badge-first format: "HOST (Windows 11 Host)"
                badge_name = first_part
                display_name = second_part
                years = badge_name  # Use badge as years for display
            else:
                # Traditional format: "30+ Years (Legends)"
                years = first_part
                display_name = second_part
                badge_name = second_part

            current_era = {
                'years': years,
                'name': display_name,
                'badge_class': badge_map.get(badge_name, 'legends'),
                'color': color_map.get(badge_name, 'gold'),
                'software': [],
                'description': ''
            }
            in_table = False

        # Detect traits section
        elif line.startswith('## ') and 'Traits' in line:
            # Save last era
            if current_era and current_table:
                current_era['software'] = parse_software_table(current_table)
                current_era['description'] = build_description_html(section_description)
                era_sections.append(current_era)
                current_table = []
                current_era = None
            in_traits = True

        # Parse traits as numbered list
        elif in_traits and line and line[0].isdigit() and '.' in line and '**' in line:
            # Parse: "1. **Title** — Description"
            parts = line.split('.', 1)
            if len(parts) > 1:
                text = parts[1].strip()
                if '**' in text:
                    title_part = text.split('**')[1]
                    desc_part = text.split('—')[1].strip() if '—' in text else ''
                    traits.append({'title': title_part, 'description': desc_part})

        # Collect table lines
        elif line.startswith('|') and not line.startswith('|---'):
            if not in_table:
                in_table = True
                current_table = [line]
            else:
                current_table.append(line)
        elif in_table and not line.startswith('|'):
            in_table = False

        # Collect section description (text between H2 and table)
        elif current_era and not in_table and not in_traits and line and not line.startswith('---') and not line.startswith('#'):
            section_description.append(line)

        i += 1

    # Save last era
    if current_era and current_table:
        current_era['software'] = parse_software_table(current_table)
        current_era['description'] = build_description_html(section_description)
        era_sections.append(current_era)

    return {
        'title': title,
        'subtitle': subtitle,
        'header_title': title,
        'intro': intro,
        'era_sections': era_sections,
        'traits': traits
    }


def build_description_html(desc_lines):
    """Build HTML from description lines"""
    if not desc_lines:
        return ''

    html_parts = []
    in_list = False

    for line in desc_lines:
        stripped = line.strip()
        if not stripped:
            continue

        # Handle bullet points
        if stripped.startswith('- '):
            if not in_list:
                html_parts.append('<ul>')
                in_list = True
            html_parts.append(f'<li>{stripped[2:]}</li>')
        else:
            if in_list:
                html_parts.append('</ul>')
                in_list = False
            # Convert bold markdown (handle ** pairs properly)
            stripped = re.sub(r'\*\*(.*?)\*\*', r'<strong>\1</strong>', stripped)
            html_parts.append(f'<p>{stripped}</p>')

    if in_list:
        html_parts.append('</ul>')

    return ''.join(html_parts) if html_parts else ''


def parse_software_table(table_lines):
    """Parse software table rows into structured data"""
    software_list = []

    for line in table_lines[1:]:  # Skip header row
        cells = [cell.strip() for cell in line.split('|')[1:-1]]
        if len(cells) >= 5:
            # Extract software name and potential URL
            name_cell = cells[0].replace('**', '').strip()

            software = {
                'name': name_cell,
                'year': cells[1].strip(),
                'years_active': cells[2].strip(),
                'category': cells[3].strip(),
                'description': cells[4].strip(),
                'url': ''  # URLs would need to be in markdown, not currently in the table
            }
            software_list.append(software)

    return software_list


def parse_index_content(body, config):
    """Parse index markdown content to extract structured data for card-based layout"""
    lines = body.split('\n')

    # Initialize variables
    title = 'Stempy Editions'
    subtitle = 'Stempy Editions'
    header_title = 'Collections on <span>Durable Software Development</span>'
    intro = 'Notes, research, and curated content'
    section_label = 'Collections'
    section_title = 'Published & Upcoming'
    articles = []

    i = 0
    current_article = {}

    # Status to CSS class mapping
    status_class_map = {
        'Live': 'live',
        'In Progress': 'soon',
        'Soon': 'soon',
        'Upcoming': 'soon'
    }

    # Accent colors
    accent_colors = config.get('accent_colors', ['gold', 'purple', 'cyan'])

    while i < len(lines):
        line = lines[i].strip()

        # Extract main title (H1)
        if line.startswith('# '):
            title = line[2:].strip()
            subtitle = title

        # Extract header title (H2 - first one becomes the header)
        elif line.startswith('## ') and not section_title:
            header_title = line[3:].strip()

        # Extract section title (H2 after "Published")
        elif line.startswith('## ') and ('Published' in line or 'Upcoming' in line):
            section_title = line[3:].strip()

        # Extract article title (H3)
        elif line.startswith('### '):
            # Save previous article if exists
            if current_article and 'title' in current_article:
                # Add accent color and status class
                article_index = len(articles)
                current_article['accent_color'] = accent_colors[article_index % len(accent_colors)]
                current_article['status_class'] = status_class_map.get(current_article.get('status', 'Live'), 'live')
                articles.append(current_article)

            current_article = {
                'title': line[4:].strip()
            }

        # Extract status and date (bold text with "Status:")
        elif line.startswith('**Status:**') or line.startswith('<strong>Status:</strong>'):
            # Parse: **Status:** Live | Updated November 2025
            status_line = line.replace('**Status:**', '').replace('<strong>Status:</strong>', '').strip()
            parts = status_line.split('|')
            if len(parts) >= 2:
                current_article['status'] = parts[0].strip()
                current_article['date'] = parts[1].replace('Updated', '').strip()

        # Extract summary (paragraph after status)
        elif line and not line.startswith('#') and not line.startswith('**') and not line.startswith('[') and not line.startswith('---') and not line.startswith('*') and current_article and 'status' in current_article and 'summary' not in current_article:
            current_article['summary'] = line

        # Extract link
        elif line.startswith('[Explore]'):
            link_match = re.search(r'\[Explore\]\((.*?)\)', line)
            if link_match:
                current_article['link'] = link_match.group(1)

        # Extract intro text (text between H2 and first ---)
        elif line and not line.startswith('#') and not line.startswith('---') and not intro and not current_article:
            if not line.startswith('**') and not line.startswith('['):
                intro = line

        i += 1

    # Save last article
    if current_article and 'title' in current_article:
        article_index = len(articles)
        current_article['accent_color'] = accent_colors[article_index % len(accent_colors)]
        current_article['status_class'] = status_class_map.get(current_article.get('status', 'Live'), 'live')
        articles.append(current_article)

    return {
        'title': title,
        'subtitle': subtitle,
        'header_title': header_title,
        'intro': intro,
        'section_label': section_label,
        'section_title': section_title,
        'articles': articles
    }


def get_output_path(source_file, config, content_type_config):
    """Determine output path for HTML file"""
    source_root = Path(config['source']['root_dir'])
    output_root = Path(config['output']['root_dir'])

    # Get relative path from source root
    try:
        rel_path = Path(source_file).relative_to(source_root)
    except ValueError:
        rel_path = Path(source_file).name

    # Remove source_path prefix if it exists
    source_path = content_type_config.get('source_path', '')
    if source_path:
        try:
            rel_path = rel_path.relative_to(source_path)
        except ValueError:
            pass

    # Add output subdirectory
    output_subdir = content_type_config.get('output_subdir', '')
    if output_subdir:
        output_path = output_root / output_subdir / rel_path.parent
    else:
        output_path = output_root / rel_path.parent

    # Change extension to .html
    output_file = output_path / f"{rel_path.stem}.html"

    return output_file


def should_exclude(file_path, config):
    """Check if file should be excluded from conversion"""
    source_root = Path(config['source']['root_dir'])

    try:
        rel_path = Path(file_path).relative_to(source_root)
    except ValueError:
        return False

    # Check excluded directories
    exclude_dirs = config['source'].get('exclude_dirs', [])
    for exclude_dir in exclude_dirs:
        if exclude_dir in rel_path.parts:
            return True

    # Check excluded files, but NEVER exclude files directly in root_dir
    # This allows index.md and other root-level files to be processed
    is_root_level = len(rel_path.parts) == 1  # File is directly in root_dir
    if not is_root_level:
        exclude_files = config['source'].get('exclude_files', [])
        if rel_path.name in exclude_files:
            return True

    return False


def find_markdown_files(root_dir, config):
    """Find all markdown files recursively"""
    md_files = []
    root_path = Path(root_dir)

    for md_file in root_path.rglob('*.md'):
        if not should_exclude(md_file, config):
            md_files.append(md_file)

    return sorted(md_files)


def find_html_files(root_dir, config):
    """Find all HTML files to copy as-is"""
    html_files = []
    root_path = Path(root_dir)

    for html_file in root_path.rglob('*.html'):
        if not should_exclude(html_file, config):
            html_files.append(html_file)

    return sorted(html_files)


def copy_html_file(source_file, config):
    """Copy HTML file as-is to output directory, preserving structure"""
    source_root = Path(config['source']['root_dir'])
    output_root = Path(config['output']['root_dir'])

    # Get relative path from source root
    try:
        rel_path = Path(source_file).relative_to(source_root)
    except ValueError:
        return None

    # Determine output path (preserve directory structure)
    output_file = output_root / rel_path

    # Create output directory
    output_file.parent.mkdir(parents=True, exist_ok=True)

    # Copy file
    import shutil
    shutil.copy2(source_file, output_file)

    return output_file


def main():
    """Main conversion function"""
    config = load_config()
    jinja_env = setup_jinja_env(config)

    source_root = Path(config['source']['root_dir'])

    if not source_root.exists():
        print(f"Error: Source directory '{source_root}' not found")
        return

    # Find all markdown files
    md_files = find_markdown_files(source_root, config)

    # Find all HTML files to copy
    html_files = find_html_files(source_root, config)

    print(f"Found {len(md_files)} markdown files to convert")
    print(f"Found {len(html_files)} HTML files to copy")
    print(f"Source directory: {source_root}")
    print(f"Output directory: {config['output']['root_dir']}")
    print()

    converted = 0
    copied = 0

    for index, md_file in enumerate(md_files):
        try:
            print(f"Converting: {md_file.relative_to(source_root)}")

            # Read markdown file
            content = md_file.read_text(encoding='utf-8')

            # Parse frontmatter and body
            frontmatter, body = parse_frontmatter(content)

            # Ensure there's always a title - fallback to formatted filename
            if 'title' not in frontmatter or not frontmatter['title']:
                frontmatter['title'] = format_filename_as_title(md_file.name)

            # Determine content type (pass content for detection)
            content_type, content_type_config = determine_content_type(md_file, config, content)
            print(f"  Type: {content_type}")

            # Prepare template data
            template_data = {
                'title': frontmatter.get('title', ''),
                'css_files': content_type_config.get('css_files', []),
                'footer_text': config.get('defaults', {}).get('footer_text', 'Stempy Articles')
            }

            # Get template name (use .j2.html if exists, fallback to .html)
            template_name = content_type_config.get('template', 'default.html')
            j2_template_name = template_name.replace('.html', '.j2.html')

            # Try Jinja2 template first, fallback to original
            try:
                template = jinja_env.get_template(j2_template_name)
            except:
                # Fallback to non-j2 template (will need manual handling)
                print(f"  Warning: No Jinja2 template found for {j2_template_name}, skipping...")
                continue

            # Process based on content type
            if content_type_config.get('is_index'):
                # Parse index content
                index_data = parse_index_content(body, config)
                template_data.update(index_data)
                template_data['footer_text'] = f'Compiled <span>{index_data.get("date", "November 2025")}</span> · New collections ship as they are ready'

            elif content_type_config.get('is_software_list'):
                # Parse software list content
                list_data = parse_software_list(body, config)
                template_data.update(list_data)
                template_data['footer_text'] = 'Compiled <span>November 2025</span> · A tribute to software that endures'

            else:
                # Standard article
                body_html = markdown_to_html(body, config)
                accent_color = get_accent_color(index, config)
                template_data.update({
                    'accent_color': accent_color,
                    'back_link': content_type_config.get('back_link', '../index.html'),
                    'date': format_date(frontmatter.get('date', '')),
                    'excerpt': frontmatter.get('excerpt', ''),
                    'body_content': body_html
                })

            # Render template
            html = template.render(**template_data)

            # Determine output path
            output_file = get_output_path(md_file, config, content_type_config)

            # Create output directory
            output_file.parent.mkdir(parents=True, exist_ok=True)

            # Write HTML file
            output_file.write_text(html, encoding='utf-8')

            print(f"  → {output_file}")
            print()

            converted += 1

        except Exception as e:
            print(f"  ERROR: {e}")
            import traceback
            traceback.print_exc()
            print()

    # Copy HTML files
    print("\n--- Copying HTML files ---\n")
    for html_file in html_files:
        try:
            print(f"Copying: {html_file.relative_to(source_root)}")

            output_file = copy_html_file(html_file, config)

            if output_file:
                print(f"  → {output_file}")
                copied += 1
            else:
                print(f"  Skipped")

            print()

        except Exception as e:
            print(f"  ERROR: {e}")
            print()

    print(f"Conversion complete! Converted {converted} of {len(md_files)} markdown files, copied {copied} of {len(html_files)} HTML files.")


if __name__ == '__main__':
    main()
