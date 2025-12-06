---
title: "What to consider in .NET Core / 5 / 6+ Projects"
categories:
  - Blog
tags:
  - .NET Core
  - .NET
  - .NET 5
---

Welcome, perhaps your relatively new to .NET Core, you may come from Java, PHP, or even the older .NET Framework.

All too often I see devs, and architects that are not completely familiar with .NET Core implement solutions that are essentially already baked into .NET Core itself. They may have used .NET Framework in the past years ago, and use the same principles applied to .NET Core. This is not a good approach to take, for one it's reinventing the wheel, usually with code that is less battle tested, and often not as extensible. It also requires extra maintenance and custom documentation, with less support.

Well here I aim to explain the core concepts in .NET Core's base layers to help you get up to speed quickly. I am assuming you already know C# to some degree, and how it plugs into .NET in general. You also understand design patterns and understand how to write clean maintainable code.

## Quick overview with .NET Framework to .NET Core

First off `.NET Core != .NET Framework` .... that's right, while they share syntactical similarities, nuget, and tooling and so on, they are essentially completely different platforms.

- .NET Framework from .NET Framework 1 - .NET Framework 4.8
- .NET Core 1.0 - .NET Core 3.1 and now just .NET 5 / .NET 6 ..... etc

**.NET Framework**  
Designed specifically for Windows OS and released around 2001. Requires Windows tooling such as Visual Studio for editing, and I.I.S for ASP.NET Web Servers, was the .NET ecosystem up until .NET Core come around.
- Closed source Proprietary framework
- Windows only development and runtimes (there were spinoffs such as Mono for other platforms, and various other runtimes)
- ASP.NET Minmum memory for a request was approx 26kb
- ASP.NET Extensibility was somewhat complex.
- Suited SOAP, WCF, XML techs

**.NET Core**  
Re-architected from the ground up for new open cross platform developments. Released 2014

- Cross-platform development and runtimes
- Open source
- Open standards
- Highly configurable and extensible
- Command line tooling infrastructure
- Lighter, significant performance improvements
- Dependency Injection is first class principle
- Web Api minimum request memory ~2kb
- designed for modern development approaches such as Web Api's, JSON, REST.
- Provided a command line tool `dnx` which was later changed to `dotnet` in later versions.

In essence, .NET 5/6/7+ is the current and future of .NET...... there is no point going back to the older .NET Framework unless legacy projects have no alternative


## Core concepts, patterns, considerations for .NET Core/5+ apps

|Item|Description|
|----|-----------|
|Abstractions           | Define interfaces| 
|Builders               | - Define a pattern to build something|
|                       | - Web Host builders|
|                       | - Configuration Builders|
|Middleware             | Defining Request / Response Pipelines |
|Configuration          | Unlike .NET Framework, Configuration in .NET Core is incredibly flexible and simple at the same time. Very simple to extend and create new ConfigurationSource(s) and Providers, you can retrieve config from file(s), databases, environment, command line and access it consistently using `IConfiguration` or `IOptions<T>` patterns |
|Command line tooling   | - `dotnet` provides access to built in compilation and runtime tooling, as well as custom built local and global tools |
|                       | - `dotnet ef` provides database migrations, updates and reverse engineering tooling |
|                       | - [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new) command for scaffolding new solutions, projects, files. Creating custom templates. |
|                       | - [dotnet tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) for console based applications |
|Dependency Injection   | Now a first class principle. Code by abstraction |
|Health Checks          | Health checking multiple services, consider app, db, custom services etc |
|Localization           | For localizing content according to a culture, different languages, time zones etc |

## Explicit Abstractions

|Item|Description|
|----|-----------|
|`IServiceCollection`   | - .NET's Dependency Injection Service registration builder |
|`IConfiguration`       | - .NET flexible overridable configuration system, gather config from multiple sources |
|`IOptions<T>`           | - Strongly typed configuration suitable for injection based on .NET's flexible configuration system|
|`ILogger<T>`           | - Genericly typed class Logging suitable for class injection |
|`ILoggingFactory`      | - Logging factory to create loggers |
|`IDistributedCache`    | - Distributed caching such as Redis, Sql, Mem, etc |
|`IHttpClientFactory`   | - Injectable factory for creating safe pooled HttpClients |
|`IHealthCheck`         | - For a custom healthcheck to plugin to healthcheck system  |
|`IStringLocalizer<T>`  | - Simple string localization with OOTB functionality, and extendable |
