---
title: "Could this be the future of web development?"
categories:
  - Blog
tags:
  - web
  - future
---

# A history lesson

For the past 25 years or so in web development, there has only ever been one language that worked natively in the browser.......**Javascript**. Sure we have had attempts at bringing in other platforms such as active-x, Adobe Flash, Microsofts Silverlight, and various other things. I have seen, and worked with a variety of all these technologies as they came and went. The problem with them is they didn't integrate with the underlying browser DOM and API's, and implemented proprietary user input rather than using the DOM, and HTML.... they were destined to die off.

We have seen hundreds of thousands, millions of web applications with Javascript frontend code.

## The introduction of server side Javascript

I remember back around 2009 when **nodejs** was introduced, with the promise of being able to run apps created in javascript browser language, outside of the browser. Now this thought was initially horrifying, and intriguring at the same time. 

Consider that back in 2009 Javascript was very different to what it is today in 2021, we didn't have many features like async/await or promises, classes, arrow functions, string interpolation, even dom selector querying like querySelector, or things like webpack, grunt, etc. AngularJS was very new and experimental.. we had to use libraries like jQuery, mootools, prototype to actually build somewhat manageable javascript based applications. 

So having JS as a potential server side language sounded bizzare at the time.

Though over the last decade, javascript has had a crazy evolution, in it's adoption, and everything built around it, and now nodejs is just a standard feature of today's modern web applications. Both front-end and back end applications are now built with it, with an assortment of tools and libraries.

The last decade saw the rise and uptake of Javascript, SPA (Single Page Applications), Docker, Microservices, Cloud, PWA and many other components that we now use regularly.

# So what comes next?

One of the biggest issues with browser development was it's limitation of a single Language for development. For any other type of software such as desktop or mobile, we could write applications in various languages such as C, C++, Java, Python, C#, VB and so on, and use their specific toolings and eco-systems for building, packaging etc.

So why couldn't we use them to write apps for the browser, it didn't make sense to have a platform that was going to be the future of application development that was still stuck on a single scripting language (JavaScript). Fortunately a new standard has been in the works over the last few years to enable that, and as of Dec 2019 was formally recommended as an open standard.... this standard is **WebAssembly** (or WASM).

**WebAssembly** effectively allows a compiled binary version of code to run directly in the browser. Whether its developed in C# (with Blazor), C, C++, Rust, or other languages, WebAssembly provides the means to run natively in the browser without any additional plugins.

Now developers can write code in any language that has compilers for webassembly.

And that compiled code

- Will be able to run at native speeds (ideally), faster than any interpreted Javascript, finally complex code processes, algorithms directly running in the browser
- Will be able to interact with the DOM, manipulate and query..... currently this requires a Javascript Interop, though I see in the future eventually the ability for WebAssembly binaries to be able to interact without the JS interop. which could improve speed quite a lot.
- Runs in a browser Sandbox, currently the Javascript one, isolated from other tabs and the host system

For more information on webassembly [check it out](https://webassembly.org/){:target="_blank"}

# And, with WebAssembly comes an exciting new platform

One of the most interesting developments with WebAssembly is projects like [wasmer](https://wasmer.io/){:target="_blank"}. This essentially provides a universal webassembly application container that runs server side. Think docker containers though significantly more lightweight.

> Codebases that can run virtually anywhere for the first time, in a web browser, on a server backend, desktop, and eventually mobile.

This is much more universal than docker is capable of, sandboxing is already handled by webassembly platform itself, and wasmer is the server side runtime component.

- Fully Sandboxed web assembly platforms
- Run on any platform, even within any language, and chipset

There was a release article about it over at [https://www.infoq.com/news/2021/01/wasmer-generally-available/](https://www.infoq.com/news/2021/01/wasmer-generally-available/){:target="_blank"} with the official blog at [https://medium.com/wasmer/wasmer-1-0-3f86ca18c043](https://medium.com/wasmer/wasmer-1-0-3f86ca18c043){:target="_blank"}.

I truly believe this is going to change development as we know it, not just web development..... in more profound ways than what nodejs and docker have done over the past decade, the new era will be WebAssembly and runtimes like Wasmer.

