---
title: "Updated dotnet web libraries to .NET 5"
categories:
  - Blog
tags:
  - .NET
  - .NET 5
  - ASP.NET
  - C#
  - .NET Core
  - .NET Standard
  - .NET Framework
---

.NET Core is an awesome platform for development, out of the box, it's lightweight, fast, and highly extensible, with good patterns to begin with. 

And now **.NET 5** brings many benefits, one of them is a *platform to unify .NET Framework, .NET Standard and .NET Core* into a single .NET 5. That's right, with .NET 5 projects going forward, you just have .NET 5. And only really need .NET Standard for older projects.

An extract from [https://www.codemag.com/Article/2010022/From-.NET-Standard-to-.NET-5](https://www.codemag.com/Article/2010022/From-.NET-Standard-to-.NET-5)

> net5.0. This is for code that runs everywhere. It combines and replaces the **netcoreapp** and **netstandard** names

Along with many other improvements such as performance. The stuff that actually matters.

Over the years, I have been updating my base layers for web applications, first in .NET Framework, than .NET Core 2.1, than .NET 3.x. The codebase has actually shrunk over the years thanks to the benefits of .NET Core. I have been streamlining code taking into account

- Minimal dependencies
- low footprint
- Upgradeability
- .NET approach

check it out at [DotNetWebLibs Github](https://github.com/stempy/DotNetWebLibs)

