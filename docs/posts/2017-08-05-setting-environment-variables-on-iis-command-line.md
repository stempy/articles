---
title: "Setting environment variables for IIS from the command line"
categories:
  - Blog
tags:
  - iis
  - environment variables
---

Nowadays many people are using cloud services to host their websites, however there is still a usage for I.I.S for website hosting.

ASP.NET core web applications it uses environment variables to determine what environment its running as, this is much better than what ASP.NET Framework based applications used for configuration, where web.config was transformed at build time.... and therefore had to have files directly modified for runtime.

ASP.NET Core by default uses `ASPNETCORE_ENVIRONMENT` as it's core env variable, it uses this to determine what appsettings file to load and ovverride specific keys from the base appsettings..... there is a lot more it can do, however thats a story for another article.

When you deploy an IIS ASPNET Core website, you want to set the environment variables on a deploy (or release stage) pipeline, this ensures consistency.... as you can create additional pipelines for new environments very quickly.

First, we want to clear any custom settings using appcmd tool:

```sh
appcmd clear config "website-name" -section:system.webServer/aspNetCore /commit:site 
```

Now, add the `ASPNETCORE_ENVIRONMENT` variable, lets say for the environment called "*dev*"

```sh
appcmd set config "website-name" -section:system.webServer/aspNetCore /+"environmentVariables.[name='ASPNETCORE_ENVIRONMENT',value='dev']" /commit:apphost
```

And an Azure Classic Release Pipelines version, for instance if you are using IIS Manage Web App task, within a stage in a release, there is a field for additional appcmd executions separated by lines.

```sh
clear config "$(Parameters.WebsiteName)" -section:system.webServer/aspNetCore /commit:site 
set config "$(Parameters.WebsiteName)" -section:system.webServer/aspNetCore /+"environmentVariables.[name='ASPNETCORE_ENVIRONMENT',value='$(Release.EnvironmentName)']" /commit:apphost
```
