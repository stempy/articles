---
title: "Optimzing .NET Core nuget references in projects"
categories:
  - Blog
tags:
  - nuget
  - dotnetcore
  - packagereference.
---

With the development of the new .NET Core we have the new nuget package referencing which no longer uses `packages.config`, rather `PackageReference`.

Now what we do with this new package referencing is not to include nuget packages in every single project, rather just the lowest level project that needs it, this will bubble up to all projects that have a project reference dependency on that project.

ie:

In the following `Project C` has the nuget package reference, and `B` and `A` automatically get the same reference. This way we don't need to include the reference in each project.

```
Project A
|
|---- Project B
|--------|  Project C
|---------------| nuget package reference N1
```

