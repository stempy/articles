---
title: "Using Fiddler with .NET Framework Winforms Apps"
categories:
  - Blog
tags:
  - Post Formats
---

Open exe config file for the environment in a text editor.


Under `<configuration>` element place

```xml
 <!-- The following section is to force use of Fiddler for all applications, including those running in service accounts -->
    <system.net>
        <defaultProxy enabled = "true"
                      useDefaultCredentials = "true">
            <proxy autoDetect="false"
                   bypassonlocal="false"
                   proxyaddress="http://127.0.0.1:8888"
                   usesystemdefault="false" />
        </defaultProxy>
    </system.net>
```
