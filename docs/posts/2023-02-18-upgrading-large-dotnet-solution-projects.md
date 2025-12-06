---
title: "Upgrading Large .NET Solutions with lots of projects"
categories:
  - Blog
tags:
  - .NET
  - .NET 7
  - .NET Upgrade
  - .NET Update
  - .NET Core
  - .NET Solution
---

<style type="text/css">
svg { width: 50%;}
</style>

<script>
document.addEventListener('DOMContentLoaded', e => {
  	// set svg size
	document.querySelectorAll('img[alt=diagram]').forEach((x)=> {
		x.style.width = "60%";
		//x.style.maxWidth = "450px";
	});
});
</script>

12/02/2023

A few days ago, was looking into how to convert a large .NET solution to .NET 7.

So
1. define the problem
2. come up with ideas to solve the problem
3. solve the problem

Important :
- Framework for the most part refers to `TargetFramework` spec in `.csproj` files. not the actual `.NET Framework` from years past before `.NET Core` except in the case where a specific `.NET Framework vx.x.x` is mentioned.

## Warning

This article describes an approach to solve a programming problem, it may seem overly verbose, and is not focused so much around a particular code or language, rather how to approach the *problem-solving* aspect of it.  

In my day to day work for most programming related concerns, I don't draw up flow-charts and pseudo-code (as it usually fits in the head)  unless it's quite a complex problem that I believe others may also have difficulty with. Or when doing architectural work related to processes.

## **Update: 18/02/2023**

As Of Feb 15th 2023, Microsoft Released a .NET upgrade assistant that can be used inside of Visual Studio to update projects to the latest version of .NET. I haven't tried this yet, however one of the issues of doing this inside VS is when dealing with large solutions VS can be quite slow, especially with resharper.

[Visual Studio Gets a .NET Upgrade Assistant](https://www.thurrott.com/dev/279377/visual-studio-gets-a-net-upgrade-assistant)


## The Problem
---

The goal is to convert all projects within a large solution to .NET 7.

By large, I mean 350 projects in a single solution
- 32 Azure Functions - 618 source files
- 9 Web applications, a couple with react, some with blazor - 667 source files
- 28 Console applications - 227 src files
- 89 Test projects - 1217 source files

With varying versions of .NET and pacakages
- .NET Standard2.0/2.1
- .NETCore 2.1/2.2/3.1/5
- Lots of different versions of nuget packages across projects

There was previously
- .NET Framework 471

Some Considerations:
- When updating any project, Visual Studio will ask to reload/refresh etc
- When updating framework versions, it's quite likely that nuget package versions will need to be updated. This is especially true with `Microsoft` specific packages that follow the framework version.  And also third-party libs that need updating to support newer framework versions.

Now, I have refactored many solutions before, across multiple solutions, upgraded from .NET framework to .net core when it became useful (ie .NET Core 2.1).  

## The Candidates for solutions
---

1. Use visual studio to change targetframeworks
2. Use resharper to try and change targetframeworks
3. Directly modify .csproj files to change target frameworks with an editor (ie vs code)

### Issues recognized with these approaches

1. Visual studio targetframework
	1. having to go into every single project at a time
	2. with every change made to a project file, visual studio refreshes itself, this can be slow across 10's or in this case hundreds of projects
	3. Quickly determined this would not work after 30 seconds.

2. Use resharper
	1. not sure if resharper has this capability, but even if so, is still affected by VS refresh after update, and resource intensive, slow across the entire solution.
	2. Deemed same issues as `(1)`

3. Directly modify `.csproj` files, I've done this before multiple times, it seems like the best solution (directly modifying .csproj). Now there are multiple ways to do this.
	1. Simple find/replace across all `.csproj` files. Ie 
		   find: `TargetFramework>netstandard2.0</TargetFramework>`
		   replace with : `TargetFramework>net7.0</TargetFramework>`
		   Repeat with each version..... this however is error-prone and tedious

4. Use the new Visual Studio .NET Upgrade Assistant from [.NET Upgrade Assistant](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.upgradeassistant)

(1) and (2) were already out due to being painfully tedious or slow, (4) runs inside VS so will suffer from any VS slow-down. (3) was the closest, so a variation of (3) to reduce human-error (mine or anyone else)

5. Write scripts, or a console app or similar to automate the process.

Ideally (5) is the conclusion to naturally come to, its a desired state for a software dev who likes to create things. However it must make viable sense to apply it. If there is a simpler solution that is much quicker, than that maybe the better approach.

## Some Pre-Requisites - Understanding the .csproj project file sdk and targetframework
---

Let's Quickly Analyze what's in a .NET project file.

Now for this context we are just going to look into .NET Core style projects (Sdk style), .NET framework project files contain a lot more information.

#### **CSharp ClassLib**

Ok lets say we create a new `classlib` project using `dotnet new classlib` with .NET7.0, here is what we get.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <RootNamespace>dotnet_proj</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Now the key points to look at are the `Sdk` and `TargetFramework` inside the `PropertyGroup` element. Our classlib shows

```xml
<Project Sdk="Microsoft.NET.Sdk">
```
and framework
```xml
<TargetFramework>net7.0</TargetFramework>
```

With *web* projects (think WebApis, MVC, etc) the Sdk will be 

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
```

#### **Existing TargetFrameworks**

In the [Problem](#the-problem) statement above, I mentioned that there were multiple versions of .NET Core to upgrade, they will typically look like the following

```xml
<!-- class libraries -->
<TargetFramework>netstandard2.0</TargetFramework>
<TargetFramework>netstandard2.1</TargetFramework>

<!-- apps of some sort, unit tests etc -->
<TargetFramework>netcoreapp2.1</TargetFramework>
<TargetFramework>netcoreapp2.2</TargetFramework>
<TargetFramework>netcoreapp3.0</TargetFramework>
<TargetFramework>netcoreapp3.1</TargetFramework>

<!-- from net5.0 onwards, can use for any project type,
replaces need for netstandard -->
<TargetFramework>net5.0</TargetFramework>

```

Now that we know that the `TargetFramework` is essentially what's needed to change .NET versions, it becomes much simpler.

**Except** for perhaps *Azure functions*, which also have an additional element, the `AzureFunctionsVersion`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>netcoreapp2.1</TargetFramework>
		<AzureFunctionsVersion>v2</AzureFunctionsVersion>
	</PropertyGroup>
    ...
</Project>
```

#### **PackageReferences**

Now the next thing is to analyze nuget package references. Remember we are using .NET Core style projects which use PackageReference elements directly in the .csproj, rather than the older `Packages.config` file that contained nuget packages and binary dll references in the `.csproj` (ie .NET Framework style project file)

```xml
<ItemGroup>
	<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.20.0" />
	<PackageReference Include="Microsoft.AspNetCore.SpaServices.Extensions" Version="3.1.25" />
	<PackageReference Include="Microsoft.AspNetCore.Authentication.AzureADB2C.UI" Version="2.2.0" />
</ItemGroup>
```

**NOTE: PackageRerences can also include version in an element tag, though is not the default**
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore">
	<Version>2.20.0</Version>
</PackageReference>
```



## The Solution
---

## Creating a high level design

Ok given the information gathered, we can have a simple process to represent what we need to do at a high level. Eventually we will be able to use A.I to translate high level requirements to code. ChatGPT3 and Github Co-pilot have demonstrated some capability to assist with this, but there is still a way to go for them.

#### Pseudocode

Given
- Project files have an Sdk element
- TargetFramework is the .NET version
- PackageReferences contain nuget packages and versions
- Azure Functions have an AzureFunctionsVersion element as version number in format `v2` etc

```
PARSE solutionfile for projects
LOOP through projects
	Open ProjectFile
		Determine ProjectType
			Is it a web project from Sdk
			Does it have AzureFunctionsVersion element
		Update TargetFramework to new version (ie 7.0)
		IF ProjectType IS AzureFunction THEN
			Update AzureFunctionsVersion to latest
		PARSE PackageReferences
		LOOP through PackageReferences
			Get Current Version of Package
			Find Latest Version of Package - from remote nuget repositories
				Cache Latest Version of Package as it may be used in other projects
			Update PackageVersion
		ENDLOOP
	Save and Close ProjectFile
ENDLOOP
```

We can visualize this approach as a series of flowcharts

**Process Project Files**

![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-1.svg)

**Process Package References**

![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-2.svg)

## Now, onto the actual approaches

Now one of the ways to automate something like this is write a console application, with parameters and so on. We used to do stuff like this decades ago.

Though my first go-to for these types of things is **LinqPad**. I write a little about it [on my .net tools page](https://stemp.dev/blog/tools-of-the-trade-net-developer/#linqpad) . The reason for this, is I want to be able to analyze things as I'm going through, dump output results etc without stepping through the debugger...... and without writing unit tests to fulfil each case. 

> Side Note: I'm fully aware of various approaches to testing, TDD and others.

So in order to update projects en-masse.

1. Close visual studio solution, otherwise any changes get picked up and refresh vs as it's happening.
2. Open up Visual Studio solution and parse to get list of project files (or get project files from directory path)

Now inside LinqPad we could just directly update, but I prefer to analyze what needs changing first, this is something I could present to other(s) aswell to show what will change if needed. There could be oddities found, unexpected results.

### Step 1. Analyze Solution File to build a collection of project files to update

1. define a collection to hold analysis results
2. loop through each project file and open
	1. check if `TargetFramework` is in an array of ones to update
	2. if so, than add to a collection with an object containing filename, currentstate (current framework version), and targetstate (new framework version) - similar to how an event-sourcing operation might look.

**Build Collection of Projects To Update Framework**
![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-3.svg)

Now we have a collection of items showing what project files need changing to update. Very simple (also do you like [Mermaid](https://mermaid.js.org/) ). 

### Step 2. Now lets execute it. Update the project files

So we have a collection of project files spread across many directories that are due for an update from Step1.

Ok so now it's simply a matter of going through the collection of project files and updating the `TargetFramework` property of the `.csproj`.

Now a `.csproj` is essentially an Xml document. this means we can use `XDocument` from [Linq To Xml](https://learn.microsoft.com/en-us/dotnet/standard/linq/linq-xml-overview) with relative ease, or the older [XmlDocument DOM](https://learn.microsoft.com/en-us/dotnet/api/system.xml.xmldocument?view=net-7.0) to process and update the project file.

> For Reference: a .NET Project file is described at Microsofts [Understanding the project file](https://learn.microsoft.com/en-us/aspnet/web-forms/overview/deployment/web-deployment-in-the-enterprise/understanding-the-project-file)

So in order to load and update a .NET `.csproj` file we can:

1. Load the `.csproj` with libaries to manipulate it as XML such as `XDocument`
2. Load some Microsoft Libraries that deal with project files
3. Consider simple string replace

So, what is the "*correct*" way to do this?.

Generally we like to find if something exists, and if it does what is required to apply it, how much learning is needed, how simple or complex is it.

We are dealing with a simple text file that happens to be an xml format, however we are just aiming to update a few strings in the file. Nothing more. So with text we can simply use Find/Replace. 

If we were doing more manipulation on the project file than using `XDocument` may be more suitable or even search for .NET specific libs for managing project files. But in this case, it's not needed.   Find/Replace aligns with the simple Visual Studio Code approach.

#### flow to update targetframeworks

So the flow is 

1. Loop through project files
	1. Open project file into memory (project files are not large, this wont be issue)
	2. Replace `TargetFramework>{CurrentFrameworkVersion}</TargetFramework` with `TargetFramework>{NewFrameworkVersion}</TargetFramework`. I add the `TargetFramework` to be more explicit.
	3. Check if `AzureFunctionsVersion` exists in projectfile and replace the version. Ie replace `AzureFunctionsVersion>v2</AzureFunctionsVersion` with `AzureFunctionsVersion>v4</AzureFunctionsVersion` (which requires .NET6+)
	4. Save file

![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-4.svg)

Very simple, now all projects will have `net7.0` (or whatever is desired). 

#### Wait a minute, what about nuget package references?

The next thing is to implement the nuget package reference updates. Now this happens on processing each project file. What we want to do is open the project file, collect the package references, and look for the latest stable version of each package, than update the package reference. 

Now analyzing package reference updates could have been done in step (1) at the cost of more analysis time (especially fetching remote version info). Though it may have simply been too much information (ie which projects to update). Remember we are dealing with 350 projects.
	   
General flow is like so:

So the new project file update would look like:

> Note: this is the general success path, obviously have to apply error checking conditions and the like

![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-5.svg)


#### Wouldnt fetching latest package version be costly api calls?

As many projects may share similar package references, we want to cache the latest package versions to reduce need to call a remote resource more than once.

![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-6.svg)

Or alternative caching strategy where we attempt to get item directly from cache without checking if cacheitem exists. Generally simpler, though depends on how cache is implemented.

![diagram](/assets/images/2023-02-18-upgrading-large-dotnet-solution-projects-7.svg)

## so, why the flowcharts, where's the code?

This article was about *solving a problem*, solving a rather simple problem. refer to the [Warning](#warning).

As programmers, we want to jump into code, get things going as soon as possible. But how do we know if the program is correct if we don't know what the end result should be. At some point we need to map out the core steps required. 

Once flows and logic have been determined, its easy to apply in pretty much any language, be it C#, Java, JavaScript/TS (with nodejs), rust, basic whatever really. 

In order to solve any problem, you first need to understand it, otherwise you end up with spaghetti, or complex pathways as your constantly adjusting/hacking code to get it to fit/work as expected. The better you understand what is required, the easier and simpler it will be to both do and explain. 

In my early development days, whenever I came across a slightly tricky problem, I would sketch it out on pen/pencil and paper, the flow and logic, eventually it got to a point where I just now do that in my head before starting on something, it makes the development process much smoother. Though diagramming/documenting is important for future reference.  

*Again for a simple problem such as this, it's not typically required, this is just to demonstrate some of it.*

Infact with this particular solution, I did the code before writing this article. 

If there is enough interest in the code itself, I'll be happy to share that, just let me know.

## Ok, so what about LinqPad?

As mentioned at the beginning of this article, I prefer LinqPad for these types of things, its just fantastic for getting results fast for experimenting and the like. So I initially wrote a collection of linqpad scripts to apply these changes, analyze projects to update, apply changes and the like and dump out results to the linqpad output window which is fantastic.

## Ok, here's what happened.

So after writing some linqpad "scripts" (I call them that because they are not the typical .cs project and execute from LinqPad ). Here is what happened.

1. Upgraded all .NET projects within a solution with 350 projects to .NET7, including updating all nuget package references to latest. The execution time was about 2-3 minutes first run, I think mainly due to fetching the latest nuget package versions from the remote nuget repo. Still much faster than it would have taken in Visual Studio alone.
2. Opened in VS 2022 - Now this is where the real work begins.
3. Tried build, expected errors, got thousands, though 90% were due to some libraries could not be built, causing cascading effect of not being able to find dlls.
4. major issues encountered
	- upgrading .net core 2.1 --> 7 .  Now I have done this in previous  projects, the biggest changes are actually from 2.x to 3.x. So looked up the migration/breaking changes (hint: there's quite a few)
	- found issues with Automapper
	- some issues with Entity Framework
