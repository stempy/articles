---
title: Test Template Includes
date: 2025-12-13
excerpt: Testing the generic template include system
card_data:
  title: Test Card
  description: This is a test card with all fields
  link: https://example.com
  image: /images/test.jpg
  tags:
    - C#
    - .NET
    - Handlebars
metrics:
  - value: 42
    label: Tests Passed
  - value: 100%
    label: Coverage
  - value: 5ms
    label: Avg Response
sidebar:
  nested:
    title: Nested Data Test
    description: This tests dot notation data extraction
---

# Test Template Includes

This document tests the new generic Handlebars template include system.

## Card Test

Testing the card partial with full data:

{% include card data="card_data" %}

## Stats Test

Testing the stats partial with metrics:

{% include stats data="metrics" %}

## Nested Data Test

Testing dot notation for nested frontmatter:

{% include card data="sidebar.nested" %}

## Error Cases

These should generate warnings and HTML comments:

### Missing Template
{% include missing_template data="card_data" %}

### Missing Data Key
{% include card data="missing_key" %}

### Missing Data Parameter
{% include card %}

## Success!

If the above sections rendered correctly, the template include system is working!
