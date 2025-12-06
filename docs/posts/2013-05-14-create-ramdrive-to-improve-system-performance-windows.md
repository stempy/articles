---
title: "Create ramdrive to improve system performance"
categories:
  - Blog
tags:
  - Post Formats
---

So, I remmember in the old days I had an Amiga 500, now with the amiga you could easily setup a ram drive for fast access, this became very important when using floppy drives, as they were incredibly slow, hard drives at the same were super expensive.

With the Advent of SSD drives, there is less need for ramdrives, however they are still incredibly useful as your internal ram is quite a lot faster than even the fastest SSD drive.

Let's dive into creating a ramdrive with imdisk on Windows

Recommended imdisk ramdrive  (FREE)
http://www.ltr-data.se/opencode.html/#ImDisk

OR 
Dataram $19 > 4gb (free for less)
http://memory.dataram.com/products-and-services/software/ramdisk


Creating Ramdrive with imdisk
http://reboot.pro/topic/15593-faqs-and-how-tos/

I want a RAM disk to be automatically created and formatted when Windows starts up.

```cmd
     imdisk -a -s 400M -m R: -p "/fs:ntfs /q /y"
```

Create batch files to autostart
NOTE: Creating multiple ramdrives with same letter (ie R:) without detaching the existing one can lead to undesirable effects for instance visual studio compilation no longer works correctly... verify only one by control panel --> imdisk

StartRamdrive.cmd
```cmd
@echo off
set RAMDRIVE=R
set RAMDRIVESIZE=400M
echo Creating Ramdrive %RAMDRIVE%: with size %RAMDRIVESIZE%....
imdisk -a -s %RAMDRIVESIZE% -m %RAMDRIVE%: -p "/fs:ntfs /q /y"
echo Setting TEMP/TMP Envrionment to new ramdrive...
md %RAMDRIVE%:\TEMP
if "%oldtmp%"=="" (
     setx oldtemp %TEMP%
     setx oldtmp %TMP%
)
setx TEMP %RAMDRIVE%:\TEMP
setx TMP %RAMDRIVE%:\TEMP
```

Ideal to stopramdrive on logoff OR shutdown, if in logoff then use gpedit.msc and select logoff scripts as below

StopRamdrive.cmd
```cmd
@echo off
if NOT "%oldtmp%"=="" (
    echo Restoring original temp/tmp environment variables to %TEMP% ....
    setx TMP %oldtmp%
    setx TEMP %oldtemp%
)
set RAMDRIVE=R
echo Detaching Ramdrive %RAMDRIVE%: ..
imdisk -d -m %RAMDRIVE%:
```

