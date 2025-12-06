---
title: "Converting Jekyll to Astro - Lessons Learned Part 1"
categories:
  - Blog
tags:
  - Jekyll
  - Astro
  - npm
  - Javascript
  - TypeScript
---

## What is Jekyll and Astro

First off, my original website was generated from Jekyll.

Jekyll and Astro are what is known as SSG - Static Site Generators. Essentially they take a variety of file sources such as markdown, and other text sources (potentially scripts) and convert that to a fully fleshed out static HTML/CSS website. Generally with less javascript and bloat so pages remain responsive and fast, with the added benefit of being able to host anywhere... github pages or netlify being a popular choice.

In some ways it is similar to the old web developments we used do 20 years ago with plain HTML/CSS..... with a big difference, these new static sites can call api's and perform other dynamic actions as they can still have javascript in them, infact with the newer SSG's you can plug in SPA's with them, react, vue, svelte etc.

Jekyll was prob one of the first SSG to arrive around 2010 from the github co-founder, and is currently the most popular. However many others have cropped up since than including Hugo, Gatsby, and others.

The thing with Jekyll is it's built with ruby, and requires ruby libraries etc installed, it's not too fast at generation, and was designed in an era before JavaScript SPA's. So I have been looking for a new SSG (Gatsby, Hugo) and so on, however they didn't seem to fit for one reason or another (build pipelines, platform choice, logic semantics, etc).

Enter Astro.build, simple, works with modern build pipelines for Javascript and it's platforms, preact, react, svelte etc with Typescript. I believe this will provide flexibilty and better build performance.

So my goal is to convert my current website using Jekyll to Astro.build....at this point I have only started with it for a few days, so will see how it goes.... here is some lessons learned.

## Converting Jekyll to Astro


## 1. Initial Setup
-----

I head on over to `https://docs.astro.build/en/getting-started/` to create a new project. 

So simple, just uses standard npm (from Node)

```
npm create astro@latest
```

I create the `blog` template option that comes up, which provides a pre-configured blog structure. I copy my original blog markdown files across into the `pages/blog` folder and it all works, a list of blog pages are shown, and can click each one...... this is fantastic, so quick.

## 2. Blog Layouts
-----

Ok, now in Jekyll there was config that specified layouts for certain types of content, ie blogs, general pages and the like.

With Astro, the documentation demonstrates how to apply a `layout` for markdown within the markdowns `frontmatter`, this works fine with a few files, but with a lot of pages it could get tedious, also changing layouts means changing in each file... not ideal.

So what I want to do is define a default layout for content within the `pages/blog` folders, so they are all consistent.

Fortunately, Astro provides [Markdown plugins](https://docs.astro.build/en/guides/markdown-content/#markdown-plugins) that allow you to add remark based plugins. [Remark](https://github.com/remarkjs/remark) appears to be a markdown transformer, and looks like the way to accomplish what I'm after.

### Creating the set blog layout functionality

After a bit of trial and error. I came up with the following.

So, I create a file `set-blob-layout.mjs` and place in the following (also logged out console output of file to debug):

As I only want to apply specific layout for blogs, need the current file path it's processing, this is in the `file.history` array.

```js
export default function remarkSetBlogLayout(options) { 
    return function (tree, file) {
        //console.log(file);
        if (file.history[0].includes('pages/blog')){
            file.data.astro.frontmatter.layout = '/src/layouts/BlogPost.astro';
        }
    }
}
```

Now the `astro.config.mjs`, I add the `remarkSetBlogLayout` function import to the `markdown` property to process `.md` files and in integrations mdx options to process `.mdx` files.

> NOTE: It's important to apply `extends` for mdx, and `extendDefaultPlugins` true for markdown to ensure Astro's default plugins are enabled.


```js
import { defineConfig } from 'astro/config';
import mdx from '@astrojs/mdx';
import remarkSetBlogLayout from './set-blog-layout.mjs';
import sitemap from '@astrojs/sitemap';

// https://astro.build/config
export default {
	site: 'https://example.com',
	integrations: [mdx({
		remarkPlugins : {
			extends: [remarkSetBlogLayout]
		}	
	}), sitemap()],
	markdown: {
		remarkPlugins: [remarkSetBlogLayout],
		extendDefaultPlugins: true
	}
};

```

**SUCCESS !!**

And now all the blog posts in markdown files have the default layout I wanted without affecting other pages. This is very nice, it did not take long to figure out once I got the remark config setup correctly with support from the awesome guys at the [Astro Support Thread](https://discord.com/channels/830184174198718474/845451724738265138) on discord.

## 3. Set publish date for blogs from the filename
----

So in my Jekyll setup, all my posts are in a single folder and the publish date is inferred from the filename, in the format:

`posts/YYYY-MM-DD-post-title.md`

> I prefer to have the explicit date in the file name as it makes organization easier at a glance. And reduces need to apply inside each file.

So there were 2 things to apply:

1. set blog post date from filename
2. being able to organize blog posts, by year into folders

So for *(1.)* I created a remark plugin to apply the `pubDate` publishDate. 

```js
export default function setPubDateFromFileRemarkPlugin() {
    return function (tree, file) {
        if (file.history 
            && file.history.length 
            && file.history[0].includes('pages/blog')){
            const histFile = file.history[0];
            const fileNameOnly = getFileName(histFile);

            try {
                const dateStr = getDateStrFromFile(histFile);
                //console.log(`Date-Found-In-FileName: file:${fileNameOnly} Date:${dateStr}`);
                file.data.astro.frontmatter.pubDate = dateStr;
            } catch(e) {
                //console.log(`Date-Not-Found: ${fileNameOnly} Could be in Meta - Error: ${e.message}`);
            };
        }

    }
  }

```

Once adding this plugin to the config arrays similar to the `set-blog-layout`, all the dates were correctly applied to all the markdown posts.

And for *(2.)* I simply moved the files into suitably organized folders.

## Conclusion

Day 1 concluded with an excellent result, and Astro is looking like a winner so far. I am keen to get more moved over. Copying the the markdown files over were literally just a copy, and just a few adjustments to apply the layout and publishing date.

