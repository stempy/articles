---
title: "Autoscraper Dev Notes 2012"
categories:
  - Blog
tags:
  - link
  - Post Formats
---

## Winforms based Web Scraper

Features:

- jQuery/CSS3 Based selectors for creating scrapers for page content
- Multi page web scraping with event driven page scraping api
- Export to multiple formats, csv, xml,  with more to come (db, other)
- Multiple websites
- Familiar Explorer like tree structure
- Built in querying for creating scrapers.

Built with:

Winforms .Net 4.0
- Generates machine hash for sending to server to get valid licence
- Scrapes pages
- Uses NetSparkle to auto-update

.Net 4.0 Web Api for licencing api and db
- Handles product activations from client app
- Generates valid product keys to activate product and stores in db
- Handles order submission from thirdparty payment processor
- Generates valid licence for client app from product key and machine hash (generated from client) to activate for particular machine. 
- Elmah and nlog for logging
- EF Code First 5

MVC 4 Web App for public website
 
Current Status:
App ready for beta public consumption
