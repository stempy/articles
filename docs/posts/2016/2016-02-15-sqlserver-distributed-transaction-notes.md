---
title: "SQL Server Distributed Transaction Notes"
categories:
  - Blog
tags:
  - Post Formats
---

- About SQL Server Distributed Transactions https://redmondmag.com/articles/2014/02/27/sql-server-distributed-transactions.aspx
- Questions relating to MSDTC (Enabled by default) http://stackoverflow.com/questions/7081823/msdtc-and-distributed-computing?rq=1
- Enable DTC Network access https://technet.microsoft.com/en-us/library/cc753510(WS.10).aspx
- Troubleshoot issues https://msdn.microsoft.com/en-us/library/aa561924.aspx
- http://stackoverflow.com/questions/10346367/mvc-3-the-msdtc-transaction-manager-was-unable-to-pull-the-transaction-from-th
- troubleshooting http://blogs.msdn.com/b/distributedservices/archive/2008/11/12/troubleshooting-msdtc-issues-with-the-dtcping-tool.aspx
- DEBUGGING http://blogs.microsoft.co.il/idof/2010/07/19/debugging-distributed-transactions-msdtc-network-problems/
- VIEWING TRACE DATA http://blogs.msdn.com/b/distributedservices/archive/2009/02/07/the-hidden-tool-msdtc-transaction-tracing.aspx
- client restrictions http://dynamicsgpland.blogspot.com.au/2009/04/more-than-i-ever-wanted-to-know-about.html
http://blogs.msdn.com/b/distributedservices/archive/2008/11/12/troubleshooting-msdtc-issues-with-the-dtcping-tool.aspx#AccessisDenied

ERROR MESSAGE 5 - ERROR MESSAGE 5 - Access is Denied
Invoking RPC method on TURTLE86 
 Problem:fail to invoke remote RPC method 
 Error(0x5) at dtcping.cpp @303 
 -->RPC pinging exception 
 -->5(Access is denied.)
This error will only occur if the destination machine is a Windows XP machine or a Windows VISTA machine. This is an additional security in the RPC layer which is configured on the client operating systems. More details on this security aspect is described in the article "RPC Interface Restriction" on Technet
- RPC Interface Restriction http://technet.microsoft.com/en-us/library/cc781010.aspx
 To get rid of this error just follow these steps to configure the registry key and REBOOT the machine.
1.  Click Start, click Run, type Regedit, and then click OK. 
 2.  Locate and then click the following registry key: 
         HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT 
 3.  On the Edit menu, point to New, and then click Key. 
         Note If the RPC registry key already exists, go to step 5. 
 4. Type RPC, and then press ENTER. 
 5. Click RPC. 
 6. On the Edit menu, point to New, and then click DWORD Value. 
 7. Type RestrictRemoteClients, and then press ENTER. 
 8. Click RestrictRemoteClients. 
 9. On the Edit menu, click Modify. 
 10. In the Value data box, type 0, and then click OK. 
         Note To enable the RestrictRemoteClients setting, type1.  
11. Close Registry Editor and restart the computer. 

- http://oysterleelife.blogspot.com.au/2013/07/solved-communication-with-underlying.html
- http://blogs.microsoft.co.il/idof/2010/07/19/debugging-distributed-transactions-msdtc-network-problems/

How to enable on a web server

- https://msdn.microsoft.com/en-us/library/dd327979.aspx
