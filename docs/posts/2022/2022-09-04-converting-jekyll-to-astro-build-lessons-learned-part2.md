---
title: "Converting Jekyll to Astro - Lessons Learned Part 2"
categories:
  - Blog
tags:
  - Jekyll
  - Astro
  - npm
  - Javascript
  - TypeScript
---

## Copying Portfolio & Blogs By Years

This follows on from [Part 1](./2022-08-27-converting-jekyll-to-astro-build-lessons-learned-part1.md) of jekyll to astro.build migration.

So now I have a basic astro.build blog site setup with dated blog posts. The next steps were to:

- Copy over portfolio pages
- Apply blog by years blog page


## Portfolio

Now this was rather simple, once I learnt about the astro specifics.

1. Copy my portfolio folder over which included `.md` files to a `/pages/portfolio` folder in astro
2. Create a `/pages/portfolio.astro` view page

In the front matter, I put TypeScript to gather the portfolio items sorted by date with 

```ts
const portfolioItems = (await Astro.glob("~/pages/portfolio/**/*.{md,mdx}")).sort(
	(a, b) => new Date(b.frontmatter.pubDate).valueOf() - new Date(a.frontmatter.pubDate).valueOf()
);
```

Than applied html containers to house the content.

And next up was adding a link to the navigation component.

This all worked rather well, and was quite simple in essence.

## Add Blog By Years

In my legacy (Jeykyll) site, I had a page that displayed blog posts grouped by years. With a multi-column grid for the years.

I wanted to re-create this in astro with some js (typescript). So here I go.

1. copy the general html layout, and some css styles
2. write some javascript to gather blog posts, than group them into years in descending date order
3. render out for the arrays of years, with arrays of posts in each.

Well this was simpler than I first figured, the main gotcha seems to be the same with any js app, typing, property naming etc. It's so nice to be able to use simple Javascript/Typescript for this, gives complete flexibility. I will prob clean everything to use typescript a bit later but for now just want to test things out.

### Writing some Javascript to group blog posts by year

Now just wrote up some simple js functions to load and group the posts that than get imported into the astro page. The end result is actually a typescript typed definition.

**Snippet**

```js
export default function groupBy(arr, prop) {
    const map = new Map(Array.from(arr, obj => [obj[prop], []]));
    arr.forEach(obj => map.get(obj[prop]).push(obj));
    return Array.from(map.values());
}

const posts = fetchPosts();   // returns sorted, filtered posts

const postYear = posts.map((p)=>{
      return { year: new Date(p.pubDate).getFullYear(), post: p};
    })

// variable to cache results
_postByYearGrp = groupBy(postYear, 'year').map(yrPost => {
    return { 
      year: yrPost[0].year, 
      posts: yrPost.map(p => p.post)
    };
});
```

### Now need taxonomy for list of years

Now to create the taxonomy for list of years

```js
export const getPostByYearTaxonomy = () => {
    return _postByYearGrp.map(byYear => {
          return {
            year: byYear.year, 
            postCount: byYear.posts.length,
            url: `#${byYear.year}`
          }
      });
}
```