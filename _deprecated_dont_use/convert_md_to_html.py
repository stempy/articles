#!/usr/bin/env python3
"""
Generic Markdown to HTML Converter with Template System
Converts all markdown files under a directory tree to HTML while preserving structure
"""

import os
import re
import yaml
from pathlib import Path
from datetime import datetime


def load_config(config_file='convert_config_generic.yml'):
    """Load configuration from YAML file"""
    config_path = Path(config_file)
    if not config_path.exists():
        raise FileNotFoundError(f"Configuration file not found: {config_file}")

    with open(config_path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)


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


def fix_image_path(path, config, rel_depth=2):
    """Convert absolute image path to relative path"""
    if not path:
        return path

    # Remove leading slash if present
    if path.startswith('/'):
        path = path[1:]

    # Use configured images base path
    images_base = config['paths']['images_base']

    # Adjust relative path based on depth
    depth_prefix = '../' * rel_depth

    return f"{depth_prefix}{path}"


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
        src = fix_image_path(src, config, rel_depth)
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
    in_table = False
    table_lines = []

    for line in lines:
        stripped = line.strip()

        # Table detection (simple markdown tables)
        if '|' in stripped and stripped.startswith('|'):
            if not in_table:
                in_table = True
                table_lines = []
            table_lines.append(stripped)
            continue
        elif in_table:
            # Process table
            html_lines.append(process_table(table_lines))
            in_table = False
            table_lines = []

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

    if in_table:
        html_lines.append(process_table(table_lines))

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


def process_table(table_lines):
    """Process markdown table into HTML"""
    if not table_lines:
        return ''

    html = ['<table>']

    for i, line in enumerate(table_lines):
        # Skip separator line (usually second line with dashes)
        if i == 1 and all(c in '-|: ' for c in line):
            continue

        cells = [cell.strip() for cell in line.split('|')[1:-1]]  # Remove empty first/last

        if i == 0:
            html.append('<thead><tr>')
            for cell in cells:
                html.append(f'<th>{cell}</th>')
            html.append('</tr></thead><tbody>')
        else:
            html.append('<tr>')
            for cell in cells:
                html.append(f'<td>{cell}</td>')
            html.append('</tr>')

    html.append('</tbody></table>')
    return '\n'.join(html)


def get_accent_color(index, config):
    """Get accent color based on index"""
    colors = config.get('accent_colors', ['gold', 'purple', 'cyan'])
    return colors[index % len(colors)]


def format_date(date_str):
    """Format date string"""
    if not date_str:
        return ''
    try:
        date_obj = datetime.strptime(str(date_str), '%Y-%m-%d')
        return date_obj.strftime('%B %Y')
    except:
        return str(date_str)


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


def load_template(template_name, config):
    """Load an HTML template file"""
    templates_dir = config['source']['templates_dir']
    template_path = Path(templates_dir) / template_name
    if not template_path.exists():
        raise FileNotFoundError(f"Template not found: {template_path}")
    return template_path.read_text(encoding='utf-8')


def create_css_links(css_files):
    """Create CSS link tags from list of CSS files"""
    links = []
    for css_file in css_files:
        links.append(f'    <link rel="stylesheet" href="{css_file}">')
    return '\n'.join(links)


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
    section_description = []  # Track description lines for current section

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
                # Generic: add span around last 3 words
                parts = title.rsplit(' ', 2)  # Split from end
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
                current_era['description'] = section_description.copy()
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
                'description': []
            }
            in_table = False

        # Detect traits section
        elif line.startswith('## ') and 'Traits' in line:
            # Save last era
            if current_era and current_table:
                current_era['software'] = parse_software_table(current_table)
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
        current_era['description'] = section_description.copy()
        era_sections.append(current_era)

    return {
        'title': title,
        'subtitle': subtitle,
        'header_title': title,
        'intro': intro,
        'era_sections': era_sections,
        'traits': traits
    }


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


def build_software_card(software, accent_color):
    """Build HTML for a single software card"""
    url_html = f'<a href="{software["url"]}" target="_blank">{software["name"]}</a>' if software['url'] else software['name']

    years_badge = f'<span class="years-badge">{software["years_active"]}</span>' if software.get('years_active') and 'year' in software['years_active'] else ''

    return f'''                    <article class="software-card" style="--card-accent: var(--accent-{accent_color});">
                        <div class="card-header">
                            <h3 class="software-name">{url_html}</h3>
                            {years_badge}
                        </div>
                        <div class="card-meta">
                            <span class="meta-item"><strong>{software["year"]}</strong></span>
                        </div>
                        <span class="card-category">{software["category"]}</span>
                        <p class="card-description">{software["description"]}</p>
                    </article>'''


def build_era_section(era):
    """Build HTML for an era section"""
    software_cards = '\n'.join([build_software_card(sw, era['color']) for sw in era['software']])

    # Build description HTML if it exists
    description_html = ''
    if era.get('description'):
        # Convert description lines to HTML (handle bold, lists, paragraphs)
        desc_lines = era['description']
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
                import re
                stripped = re.sub(r'\*\*(.*?)\*\*', r'<strong>\1</strong>', stripped)
                html_parts.append(f'<p>{stripped}</p>')

        if in_list:
            html_parts.append('</ul>')

        if html_parts:
            description_html = f'''                <div class="section-description">
                    {''.join(html_parts)}
                </div>'''

    return f'''            <section class="era-section">
                <div class="era-header">
                    <span class="era-badge {era["badge_class"]}">{era["years"]}</span>
                    <h2 class="era-title">{era["name"]}</h2>
                </div>
{description_html}
                <div class="software-grid">
{software_cards}
                </div>
            </section>'''


def build_traits_section(traits):
    """Build HTML for the traits section"""
    if not traits:
        return ''

    trait_items = []
    for i, trait in enumerate(traits, 1):
        trait_items.append(f'''                    <div class="trait">
                        <span class="trait-number">{i}</span>
                        <div class="trait-content">
                            <h4>{trait["title"]}</h4>
                            <p>{trait["description"]}</p>
                        </div>
                    </div>''')

    return f'''            <section class="era-section">
                <div class="era-header">
                    <h2 class="era-title">Common Traits of Long-Lasting Software</h2>
                </div>
                <div class="traits-grid">
{chr(10).join(trait_items)}
                </div>
            </section>'''


def build_html(frontmatter, body, body_html, accent_color, content_type_config, config):
    """Build complete HTML page using template"""
    template_name = content_type_config.get('template', 'default.html')
    template = load_template(template_name, config)

    # Extract common frontmatter fields
    # Title is guaranteed to be present by parse_frontmatter + fallback in main()
    title = frontmatter.get('title', '')
    excerpt = frontmatter.get('excerpt', '')
    date = frontmatter.get('date', '')
    formatted_date = format_date(date)

    # Build CSS links
    css_files = content_type_config.get('css_files', [])
    css_links = create_css_links(css_files)

    # Build back link
    back_link = content_type_config.get('back_link', '../index.html')

    # Build excerpt HTML
    excerpt_html = f'<p class="article-excerpt">{excerpt}</p>' if excerpt else ''

    # Footer text
    footer_text = config.get('defaults', {}).get('footer_text', '')

    # Check content type
    is_index = content_type_config.get('is_index', False)
    is_software_list = content_type_config.get('is_software_list', False)

    if is_index:
        # Parse index content to extract structured data
        index_data = parse_index_content(body, config)

        # Build article cards
        article_cards_html = []
        accent_colors = config.get('accent_colors', ['gold', 'purple', 'cyan'])
        for i, article in enumerate(index_data['articles']):
            card_accent = accent_colors[i % len(accent_colors)]
            article_cards_html.append(build_article_card(article, card_accent))

        # Replace index-specific placeholders
        html = template.replace('{{title}}', index_data['title'])
        html = html.replace('{{subtitle}}', index_data['subtitle'])
        html = html.replace('{{header_title}}', index_data['header_title'])
        html = html.replace('{{intro}}', index_data['intro'])
        html = html.replace('{{section_label}}', index_data['section_label'])
        html = html.replace('{{section_title}}', index_data['section_title'])
        html = html.replace('{{article_cards}}', '\n'.join(article_cards_html))
        html = html.replace('{{footer_text}}', f'Compiled <span>{index_data.get("date", "November 2025")}</span> · New collections ship as they are ready')
        html = html.replace('{{css_links}}', css_links)

    elif is_software_list:
        # Parse software list content
        list_data = parse_software_list(body, config)

        # Build era sections
        era_sections_html = []
        for era in list_data['era_sections']:
            era_sections_html.append(build_era_section(era))

        # Add traits section if exists
        if list_data['traits']:
            era_sections_html.append(build_traits_section(list_data['traits']))

        # Format the title with proper HTML
        formatted_title = title
        if 'Test of Time' in title:
            formatted_title = 'Software That Stands<br>the <span>Test of Time</span>'
        elif 'Tool Setup' in title:
            formatted_title = 'My Local <span>Tool Setup</span>'
        else:
            # Generic: add span around last 2-3 words
            parts = title.rsplit(' ', 2)
            if len(parts) > 1:
                formatted_title = ' '.join(parts[:-2]) + ' <span>' + ' '.join(parts[-2:]) + '</span>'

        # Replace software list placeholders
        html = template.replace('{{title}}', title)
        html = html.replace('{{subtitle}}', list_data['subtitle'])
        html = html.replace('{{header_title}}', formatted_title)
        html = html.replace('{{intro}}', list_data['intro'])
        html = html.replace('{{era_sections}}', '\n'.join(era_sections_html))
        html = html.replace('{{footer_text}}', 'Compiled <span>November 2025</span> · A tribute to software that endures')
        html = html.replace('{{css_links}}', css_links)

    else:
        # Standard template replacement
        html = template.replace('{{title}}', title)
        html = html.replace('{{accent_color}}', accent_color)
        html = html.replace('{{css_links}}', css_links)
        html = html.replace('{{back_link}}', back_link)
        html = html.replace('{{date}}', formatted_date)
        html = html.replace('{{excerpt}}', excerpt_html)
        html = html.replace('{{body_content}}', body_html)
        html = html.replace('{{footer_text}}', footer_text)
        html = html.replace('{{formatted_date}}', formatted_date)

        # Portfolio-specific fields
        if 'meta_items' in template:
            meta_items = build_portfolio_meta(frontmatter)
            html = html.replace('{{meta_items}}', meta_items)

        if 'tech_tags' in template:
            tech_tags = build_tech_tags(frontmatter)
            html = html.replace('{{tech_tags}}', tech_tags)

        if '{{hero_image}}' in template:
            html = html.replace('{{hero_image}}', '')

        if '{{gallery}}' in template:
            html = html.replace('{{gallery}}', '')

    return html


def build_portfolio_meta(frontmatter):
    """Build portfolio meta items from frontmatter"""
    meta_items = []

    # Try to extract from sidebar structure
    sidebar = frontmatter.get('sidebar', [])
    if isinstance(sidebar, list):
        for item in sidebar:
            if isinstance(item, dict):
                title = item.get('title', '')
                text = item.get('text', '')
                if title and text:
                    meta_items.append(
                        f'<div class="portfolio-meta-item">'
                        f'<span class="portfolio-meta-label">{title}:</span>'
                        f'<span>{text}</span></div>'
                    )

    return '\n                '.join(meta_items)


def build_tech_tags(frontmatter):
    """Build tech tags from frontmatter"""
    tags = frontmatter.get('tags', [])
    if isinstance(tags, list) and tags:
        tag_spans = [f'<span class="tech-tag">{tag}</span>' for tag in tags]
        return '<div class="tech-tags">' + ''.join(tag_spans) + '</div>'
    return ''


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
        elif line.startswith('## ') and 'Published' in line or 'Upcoming' in line:
            section_title = line[3:].strip()

        # Extract article title (H3)
        elif line.startswith('### '):
            # Save previous article if exists
            if current_article and 'title' in current_article:
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


def build_article_card(article, accent_color):
    """Build HTML for a single article card"""
    # Map status to CSS class
    status_class_map = {
        'Live': 'live',
        'In Progress': 'soon',
        'Soon': 'soon',
        'Upcoming': 'soon'
    }

    status = article.get('status', 'Live')
    status_class = status_class_map.get(status, 'live')
    date = article.get('date', '')
    title = article.get('title', '')
    summary = article.get('summary', '')
    link = article.get('link', '#')

    return f'''                    <article class="software-card article-card" style="--card-accent: var(--accent-{accent_color});">
                        <div class="article-meta">
                            <span class="article-status {status_class}">{status}</span>
                            <span>Updated {date}</span>
                        </div>
                        <h3 class="article-title">{title}</h3>
                        <p class="article-summary">
                            {summary}
                        </p>
                        <a class="article-link" href="{link}">Explore</a>
                    </article>'''


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


def main():
    """Main conversion function"""
    config = load_config()

    source_root = Path(config['source']['root_dir'])

    if not source_root.exists():
        print(f"Error: Source directory '{source_root}' not found")
        return

    # Find all markdown files
    md_files = find_markdown_files(source_root, config)

    print(f"Found {len(md_files)} markdown files to convert")
    print(f"Source directory: {source_root}")
    print(f"Output directory: {config['output']['root_dir']}")
    print()

    converted = 0

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

            # Convert markdown to HTML
            body_html = markdown_to_html(body, config)

            # Get accent color
            accent_color = get_accent_color(index, config)

            # Build HTML
            html = build_html(frontmatter, body, body_html, accent_color,
                            content_type_config, config)

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
            print()

    print(f"Conversion complete! Converted {converted} of {len(md_files)} files.")


if __name__ == '__main__':
    main()
