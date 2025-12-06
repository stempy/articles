---
title: "Get MSBuild Path from Registry"
categories:
  - Blog
tags:
  - Post Formats
---

http://stackoverflow.com/questions/328017/path-to-msbuild

```
reg.exe query 'HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0' /v MSBuildToolsPath
```