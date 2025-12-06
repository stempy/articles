---
title: "Improve processes"
categories:
  - Blog
tags:
  - link
  - Post Formats
---

I have done a fair number of functional / technical specs for various projects, and when working on a specific project for a company, I was a little surprised at how vague documentation was. 

This may be nit-picking however I have found that projects that are clearly defined require less back and forth juggling (ie go discuss with designer, developer, project manager for details), I realize how important it is to clarify with each other the details, though if the specs are well defined from the start, less of this is required.

Requirements are much costlier to change during development than the design phase.

Specs
- Written flow of processes (i.e User fills form --> Form details submitted --> email sent --> db stores details)
  
- Possible flow charts for complex business rules
- UML Modelling even if needed, also use cases
  
This is to clarify for management the processes, management should be able to revise it at this point, before development begins, there really shouldnt be a need for micromanagement during the development stage.

Structure Project down into deliverables, phases(or stages), with concrete deliverables, 
- Inhouse development/build server, shared database server, get ftp, smtp servers setup prior to starting dev.
  
- PC's set up for remote debugging with I.I.S (either on local machine, or possible remote), once set up properly, the time saved with debugging can be spent on other areas (apparently this is difficult, though it takes about 15 mins, setting up vs, i.i.s and permissions, though in many cases it just doesnt work, but at least try and get it working...its a one off)
  
- Wireframes should have simple flow structure, for instance when user clicks item, fills form out etc, the flow of pages after that, ie thank you page... selected contacts page.
- Notifications when migrating from dev --> staging --> prod servers would be nice.... email would be ideal, or time/set on whiteboard, something clearly visible
- Break the mountain down into chunks one by one
Use tools:
- Proper BugTracker (trac, bugzilla, flyspray) to process bugs, feature requests, enhancements etc
Standardization
- Just because one developer likes a particular method of doing things, does not mean it will be effective across the board. St
  
Logins & Passwords
- Centralized storage of logins and passwords, for example use keepass (password app) with shared database over lan to store common logins
- SMTP Server Username/Passwords
- Database Connection Strings(servers, usernames & passwords)
- Remote Desktop Logins
- Virtual PC

