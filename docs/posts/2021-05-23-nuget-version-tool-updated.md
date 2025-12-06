---
title: "NugetVersion tool updated to 1.0.5"
categories:
  - Blog
tags:
  - nuget
  - dotnet tool
  - dotnet global tool
---

Over the years I have refactored many large codebases across multiple solutions, with this often comes updating of nuget packages, while you can use the VS gui tools to manage and update versions, it is limited to a solution at a time. Also it can sometimes get "stuck" and not be able to update nuget packages.

So I created a tool to help query and update nuget package versions across multiple projects below a folder structure (rather than solution), it uses the `dotnet` command line tool to perform the actual updating.

I just updated NugetVersion global tool to 1.0.5, this includes:

- Code cleanup
- Simplified command line parsing
- show project framework
- added some unit tests (more to come)

Please note: currently works with packagereference style projects. And this uses the `dotnet` commands for updating packages, so fairly safe to use.

Check it out at [NugetVersion](https://www.nuget.org/packages/NugetVersion/) and the [Github Repo for NugetVersion](https://github.com/stempy/nugetversion)

Some examples:

## Current directory

![Current Directory](/assets/images/nugetversion_currentdirectory.png)

## Current directory with name filter

![Name filter](/assets/images/nugetversion_currentdirectory_with_name_filter.png)

## Current directory with name and version filter

![Name and version filter](/assets/images/nugetversion_currentdirectory_with_name_and_version_filter.png)




