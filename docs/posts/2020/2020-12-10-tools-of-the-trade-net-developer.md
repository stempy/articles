---
title: "Essential Tools of the trade - C# .NET Developer"
categories:
  - Blog

galleryazstor:
 - url: https://azurecomcdn.azureedge.net/cvt-6a09cf6ca31b21bd1fcbf756f79b40b05abe6d97e5409f3203a7b001c65ec9f6/images/page/features/storage-explorer/valueProp1.png
   image_path: https://azurecomcdn.azureedge.net/cvt-6a09cf6ca31b21bd1fcbf756f79b40b05abe6d97e5409f3203a7b001c65ec9f6/images/page/features/storage-explorer/valueProp1.png
   alt: "placeholder image 1"
 - url: https://azurecomcdn.azureedge.net/cvt-6a09cf6ca31b21bd1fcbf756f79b40b05abe6d97e5409f3203a7b001c65ec9f6/images/page/features/storage-explorer/valueProp2.png
   image_path: https://azurecomcdn.azureedge.net/cvt-6a09cf6ca31b21bd1fcbf756f79b40b05abe6d97e5409f3203a7b001c65ec9f6/images/page/features/storage-explorer/valueProp2.png
   alt: "placeholder image 2"
 - url: https://azurecomcdn.azureedge.net/cvt-6a09cf6ca31b21bd1fcbf756f79b40b05abe6d97e5409f3203a7b001c65ec9f6/images/page/features/storage-explorer/valueProp3.png
   image_path: https://azurecomcdn.azureedge.net/cvt-6a09cf6ca31b21bd1fcbf756f79b40b05abe6d97e5409f3203a7b001c65ec9f6/images/page/features/storage-explorer/valueProp3.png
   alt: "placeholder image 3"

gallerylinqpad:
 - url: https://www.linqpad.net/images/thebigdump.png
   image_path: https://www.linqpad.net/images/thebigdump.png
   alt: "placeholder image 1"
 - url: https://www.linqpad.net/images/maincodescratchpad.png
   image_path: https://www.linqpad.net/images/maincodescratchpad.png
   alt: "placeholder image 2"
 - url: https://www.linqpad.net/images/mainscreenshot.png
   image_path: https://www.linqpad.net/images/mainscreenshot.png
   alt: "placeholder image 3"

galleryresharper:
 - url: https://www.jetbrains.com/resharper/img/screenshots/code-analysis.png
   image_path: https://www.jetbrains.com/resharper/img/screenshots/code-analysis.png
   alt: "placeholder image 1"
 - url: https://www.jetbrains.com/resharper/img/screenshots/refactorings-and-navigation.png
   image_path: https://www.jetbrains.com/resharper/img/screenshots/refactorings-and-navigation.png
   alt: "placeholder image 2"

galleryvs:
 - url: https://visualstudio.microsoft.com/wp-content/uploads/2020/11/VS2019-screenshot.png
   image_path: https://visualstudio.microsoft.com/wp-content/uploads/2020/11/VS2019-screenshot.png
   alt: "placeholder image 1"

galleryfiddle:
 - url: /assets/images/netfiddleonline.png
   image_path: /assets/images/netfiddleonline.png
   alt: "placeholder image 1"


tags:
  - Resharper
  - Refactoring
  - Azure Storage Explorer
  - LinqPad
  - Visual Studio
  - .NET
  - Azure
  - Linq
  - SQL
  - Debugging
---

Every type of development has certain tools that they use, some are the standard, and some can significantly help with development, or deployment, or debugging and testing. Here I talk about some **invaluable** tools for .NET developers that will very likely change the way you develop.


> Please Note: I am not affiliated with any of the mentioned products or companies in any way. I have just been using them for a very long time.

## Visual Studio

{% include gallery id="galleryvs" caption="Visual Studio." %}

This is the standard IDE for any .NET developer, in previous years with .NET Framework it was quite a costly IDE, now however it is free (Community), so go ahead and [download visual studio](https://visualstudio.microsoft.com/downloads/){:target="_blank"}

It's essentially a feature-rich, editing and debugging environment for development.


## Resharper

{% include gallery id="galleryresharper" caption="Resharper." %}

*powerful* productivity powerhouse plugin for Visual Studio that can significantly enhance editing, code analysis, refactoring and coding conventions. I have been using resharper since 2008.  It's feature set has proven to be so good, that the visual studio team have implemented some of it's features, though resharper goes much further than even the latest version of visual studio. 

It will improve your knowledge of C# best practices, and speed things up.

It's one of those tools once you get familiar with it, you don't ever want to go without it. It does cost, however that cost is offset by the time it can save.

[download resharper](https://www.jetbrains.com/resharper/){:target="_blank"}

## LinqPad

{% include gallery id="gallerylinqpad" caption="LinqPad." %}

Described as the **.NET Programmers Playground**, this is the **ultimate scratchpad tool** for .NET development.

Many many moons ago, developers would write console applications, or unit tests in order to experiment with new appraoches, libraries, processes, that is of course until they started using LinqPad.

Don't let the name fool you. It's not only for Linq queries, although it can easily do that. This tool will let you do practically anything with .NET, from Database queries, (and updates) with Linq, SQL, connect to other types of providers etc.... to writing complete programs. That can be run from LinqPad itself, or via the command line with it's built-in runner

The free version is very limited, *buy* the pro version (one off cost) however and get Nuget package support, full intellisense, and debugging (with step through, variable views and so on).

There is compatibility with .NET Framework, and with LinqPad 6+ .NET Core, and just recently the new .NET 5.

*buy* it (as the free version is very limited), or get your company to buy it...it's worth it.

[download linqpad](https://www.linqpad.net/){:target="_blank"}




## Azure Storage Explorer

{% include gallery id="galleryazstor" caption="Azure Storage Explorer." %}

Not just for .NET developers, however as .NET development often goes along with Azure. This *free* tool from Microsoft will let you explore, query, and modify azure based resources such as containers, blobs, queues, file shares, tables, even cosmos db.

Oh, and it's cross-platform... win win.

Just [download azure storage explorer](https://azure.microsoft.com/en-us/features/storage-explorer/){:target="_blank"}

## Other tools

There are several other tools that are useful for all types of development which I will go into in another post.

## .NET Fiddle

{% include gallery id="galleryfiddle" caption=".NET Fiddle" %}

https://dotnetfiddle.net/

