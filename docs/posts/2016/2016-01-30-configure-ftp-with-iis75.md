---
title: "Configure ftp with iis manager users 7.5"
categories:
  - Blog
tags:
  - link
  - Post Formats
---

http://www.iis.net/learn/publish/using-the-ftp-service/configure-ftp-with-iis-manager-authentication-in-iis-7

In order to use IIS Isolation properly you must create the following folder structure
http://www.iis.net/learn/publish/using-the-ftp-service/configuring-ftp-user-isolation-in-iis-7


|User Account Types          | Physical Home Directory Syntax  |
|----------------------------|---------------------------------|
|Anonymous users             | %FtpRoot%\LocalUser\Public      |
|Local Windows user accounts (requires basic authentication) |%FtpRoot%\LocalUser\%UserName% |
|Windows domain accounts (requires basic authentication)     |%FtpRoot%\%UserDomain%\%UserName% |
|IIS Manager or ASP.NET custom authentication user accounts  |%FtpRoot%\LocalUser\%UserName% |

(Note: In the above table, %FtpRoot% is the root directory for your FTP site; for example, C:\Inetpub\Ftproot.)


