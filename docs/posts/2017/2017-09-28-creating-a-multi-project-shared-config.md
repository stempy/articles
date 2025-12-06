---
title: "Creating a multi project, multi platform shared configuration"
categories:
  - Blog
tags:
  - link
  - configuration
  - Post Formats
---

Problem: 

Multiple Web / Console projects need to share same configurations for different environments, for example, dev/uat/stage/live. 

The following types of applications:
- .NET Framework desktop
- .NET Framework console
- .NET Framework web services
- ASP.NET Framework MVC / Web API
- ASP.NET Core WebApi's
- NodeJS frontends

The problem we encountered using standard ASP.NET approaches.

Issue #1

ASP.NET Projects have publish profiles which can have a web.config transform for each profile (ie dev/uat/stage etc). This needs to be done for every single web project and replicated. 

Issue #2

Non ASP.NET projects dont auto transform config files, however this is easily done by inserting TransformXML into the projects .csproj file.

Issue #3

Default config transform approaches require adding transforms directly to each project, therefore when you need to add a new environment or modify an existing one, you need to update each project individually....this doesnt work so great when you have 5 or more projects requiring exact same config data.  Now we dont want to store connection strings in any database or remote store.

Issue #4

.NET Framework and .NET Core have different configuration Approaches (hint .NET Core is superior).

- .NET Framework uses Web.config with config Transforms with a fairly rigid configuration system, Config transforms would actually modify the end Web.config based on a publish profile and would suit a specific deployed environment. Configs are in XML format.

- .NET Core uses appsettings.json by default, and a very flexible, extensible configuration system. Rather than modifying an existing config file for specific deployments. It uses an ovveride configuration system. In that it can load configuration from multiple sources (think file,web,mem,environment,command line) and can apply configuration in the order it's loaded. no config files are actually modified per say. It's more suited to CI/CD scenarios.

I didnt want to duplicate appsettings or connection strings across project transform files, so I created a shared Configs folder, and linked connectionstring and appsetting file for all environments into a Configs dir within each project

