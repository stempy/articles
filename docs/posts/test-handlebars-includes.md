---
title: Test Handlebars Include Rendering
date: 2025-12-13
excerpt: Comprehensive test of template include functionality with various frontmatter formats

# Simple flat gallery
gallery:
  - image_path: /images/test1.jpg
    alt: Test image 1
    title: First test image
  - image_path: /images/test2.jpg
    alt: Test image 2
    title: Second test image
  - image_path: /images/test3.jpg
    alt: Test image 3
    title: Third test image

# Named gallery with custom layout
gallery_custom:
  - image_path: /images/custom1.jpg
    alt: Custom 1
  - image_path: /images/custom2.jpg
    alt: Custom 2

# Card data - single card
feature_card:
  title: Featured Project
  description: This is a test card with all fields populated
  image: /images/feature.jpg
  link: https://example.com
  tags:
    - tag1
    - tag2
    - tag3

# Card data - multiple cards
project_cards:
  - title: Project One
    description: First project description
    image: /images/project1.jpg
    link: /portfolio/project1.html
    tags:
      - web
      - react
  - title: Project Two
    description: Second project description
    image: /images/project2.jpg
    link: /portfolio/project2.html
    tags:
      - mobile
      - swift
  - title: Project Three
    description: Third project without image
    link: /portfolio/project3.html
    tags:
      - cli

# Stats - simple metrics
basic_stats:
  - value: "1.2M"
    label: Downloads
  - value: "4.8"
    label: Rating
  - value: "523"
    label: Contributors

# Nested data structure
sidebar:
  metrics:
    - value: "99.9%"
      label: Uptime
    - value: "< 100ms"
      label: Response Time
    - value: "24/7"
      label: Support

# Edge cases
empty_gallery: []

single_stat:
  - value: "42"
    label: The Answer
---

# Test Handlebars Include Rendering

This document tests various template include scenarios with different frontmatter formats.

## Gallery Tests

### Default Gallery (auto-layout with 3 items)

{% include gallery %}

### Custom Gallery with ID and Half Layout

{% include gallery id="gallery_custom" layout="half" caption="Custom Gallery Example" %}

### Gallery with Custom Class

{% include gallery id="gallery" class="my-custom-gallery" %}

## Card Tests

### Single Feature Card

{% include card data="feature_card" %}

### Multiple Project Cards

This tests if card includes work with array iteration in markdown (may need multiple includes):

{% include card data="project_cards" %}

## Stats Tests

### Basic Stats Grid

{% include stats data="basic_stats" %}

### Nested Stats (using dot notation)

{% include stats data="sidebar.metrics" %}

### Single Stat

{% include stats data="single_stat" %}

## Edge Cases

### Empty Gallery

{% include gallery id="empty_gallery" %}

### Non-existent Data Key

{% include card data="nonexistent_key" %}

### Missing Template

{% include nonexistent_template data="feature_card" %}

### Unquoted Parameter (should fail gracefully)

This is intentionally malformed to test error handling:
`{% include card data=feature_card %}`

## Nested Content Example

Here's a gallery embedded in a complex markdown structure:

> **Quote with Gallery**
>
> {% include gallery id="gallery_custom" layout="half" %}

- List item 1
- List item with gallery:
  {% include gallery %}
- List item 3

### Code Block Test

```markdown
{% include gallery %}
```

This should NOT be processed (inside code block).

## Multiple Includes in Sequence

{% include stats data="basic_stats" %}

{% include card data="feature_card" %}

{% include gallery layout="third" %}

## Conclusion

This test file validates:
- ✅ Simple and nested frontmatter structures
- ✅ Arrays of objects
- ✅ Different built-in partials (gallery, card, stats)
- ✅ Parameter passing with quotes
- ✅ Dot notation for nested data access
- ✅ Edge cases and error handling
- ✅ Multiple includes in one document
