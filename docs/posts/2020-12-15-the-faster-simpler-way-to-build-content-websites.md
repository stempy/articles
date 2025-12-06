---
title: "The faster, simpler way to build content driven websites"
categories:
  - Blog

tags:
  - CMS
  - Content Management System
  - Wordpress
  - Static Site Generators
  - SSG

---

## The Content Management System 

Early on in my career, I have implemented various CMS applications for simple content-driven websites, by content-driven I mean data would be stored in a database, with a Wysiwyg editor. 

It was a big step up from static HTML pages and made it easy to add and edit content for websites, though it also increased complexity and costs more, as you needed a database, configuration set, learning how to build themes and modules as the CMS systems became more advanced.

I had gone through very simple CMS's to monolithic and complex data-driven systems for enterprise systems, that were overly complicated for the task, which was to deliver HTML pages to a website.

Consider some of these CMS, PhpNuke, Xoop, Mambo and than Joomla, Drupal, Wordpress, Concrete5, Modx, DotNetNuke, Umbraco, SiteCore, and other custom built CMS are some of the ones I have implemented, each with their own approaches, theming and module design. with a dizzying array of plugins and features.

## Purpose of a CMS

Now, here's what I learnt through all that, most of these CMS's were designed so an end user, or groups of users could edit their own content on their webpages in a wysiwyg.... 

And what actually happens is the html placed into the wysiwyg doesn't fit in with the design, or looks wierd, it's copy and pasted from word, and so on.... and often many times, the task to edit content would be shifted back to the web developers, or someone who knows html a bit better.

> And if you know html, what is the point of a WYSIWYG editor?

Now, about 80% of the worlds websites seem to be fairly simple and small websites, driven by a CMS such as Wordpress, with updates continually needed to keep hackers at bay, plugins and modules in order to make their site look nice.

## Enter the Static Site Generator (SSG)

Yeah, many of these type of websites could be replaced with a SSG (Static Site Generator). Which effectively generate a complete website in one build step similar to how web applications are built, and similar to the old days of plain HTML, CSS, and Javascript. What is different however is how they are generated.

In the old days of editing HTML, you had to copy html pages, any changes to layout had to be applied to every single HTML page, which was tedious and never worked well for more than half a dozen pages or so. (hence the reason for CMS's)

With SSG's you use layouts and a combination of [markdown]{:target="_blank"}(https://www.markdownguide.org/getting-started/) and html files, and some wizadry to process these files into a complete site, with a templating language to provide all sorts of capability.

The most popular SSG is **Jekyll** which Github Pages uses.

## Jekyll - The Current Number One SSG

Well, jekyll has a few quirks, but you only need to learn it once. For general content sites,  which  don't have any real workflow, its great..... learn it once, rinse and repeat.

for starters, you have 

- posts, which are like a blog, and than
- pages, which provide the  general content, say about us, what we do, how we can help you, contact page
- collections, which provide the  custom page types, such as a portfolio, recipes, pets, etc.

And

## SEO works the best

friendly urls out of the box, static content may have potentially higher search engine ratings..

## Fast

html, css, and javascript is the end result of all modern web u.i's, regardless of server, or frontend framework..... html is the end result, that's where SSG's wins, it generates html in one go before it's served, skips the pipelines needed to generate html., so it's always ready.

## It's hard to hack something that is just text

- there's no authentication
- no backend database, infact no backend at all besides the server.
- just plain html/css/js served

## Low on-going hosting costs

lowest cost hosting, as low as free....
- there are several free hosting providers for static sites
- github pages
- netlify

## Content is stored by you

*Content is King*.

In a CMS, Content is stored in a database, which must be backed up frequently, can grow. If you want to migrate content over to another CMS that can be a task in itself.

With SSG's such as Jekyll, content is stored as plain text in markdown/html format, on your computer, in a repository in the cloud. Whereever you want.

## What's not to like

Well there are some great reasons to use jekyll and SSG's, and all the benefits it provides above are very compelling, however when a website requires more than just static content, and needs some type of workflow, a more capable system may be required.
