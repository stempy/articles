---
title: "iaAnyware"
tags:
  - ASP.NET
  - .NET Core
  - .NET Framework
  - .NET
  - C#
  - Javascript
  - Aurelia
  - Azure Cloud
  - Azure Devops
  - Architecture
  - VSTS
  - Crystal Reports
  - Entity Framework
  - NodeJs
  - MongoDb
  - Metronic
  - jQuery
  
---

August 2012 - July 2019 (6 month contracted extended to 7 years)

iaAnyware had built a winforms ASP.NET 3.5 Desktop application to provide line of business applications for brokers to manage clients, quotes, policies, and claims information.

I introduced web development, and streamlined development processes, full lifecycle development from conception to development, to training web development concepts, architecture, infrastructure, ci/cd. Effectively DevOps And Software Architecture, along with backend and frontend development.

- Unit testing with nunit/xunit
- Web APIs (prev SOAP)
- [Continuous Integration and Delivery prev manual deployments](https://github.com/stempy/cloud-architecture/blob/master/what-azure-devops-and-ci-provide.md)
- ASP.NET MVC
- Frontend development with javascript based components and apps
- S.O.L.I.D and best practice programming principles, clean coding
- Introduced entity framework
- Introduced and taught LINQ
- Modern Dev tools LinqPad, Postman, Slack, Trello.
- Taught javascript, html and css to other developers.
- Researched and recommended approaches and design paradigms to problems.
- Introduced Git, and GitFlow versioning
- Implemented Azure cloud services, azure websites, blobs, queues.
- Upgraded legacy code to .NET core
- Rearchitected legacy approaches to modern web based
- Developed lots of batch, powershell scripts

From 2012 to about 2015 I created web libraries and architecture for their Web based version, I created all the libraries, repositories in Git, build and deployments with MsBuild scripts and batch files. And trained developers basic html,css,js.  From there other developers became involved in the web development, and than as I migrated the ASP.NET Framework code to .NET Core, the web development became the main focus.

## Web Architecture
--------------------------

The initial application was a giant monolithic desktop winforms application, with other desktop apps for administration and other features... and soap web services to serve data. These applications were targeted to insurance brokers, and not end clients.

The new web architecture consisted of splitting components down into respective microservices, and creating Web API's suited for each part. Initially ASP.NET Framework, and later migrated to ASP.NET Core

- Authentication
- Report Generation
- Client Management
- Searching / Queries
- Document Management

Along with this, iaAnyware desired an online quoting application for new customers, and a client portal specifically for clients of the brokers to self-serve.

- Online quoting application
- Client Portal

### Platforms

- ASP.NET Framework initially than migrated about 25 projects to .NET Core including libraries and Web backends.
- ES5 Javascript with prototyped functions, than scripted processes to convert to ES6 based classes.
- Entity Framework 5.0 initially, than EF 6, and migrated to EF Core 2.1
- I.I.S hosted on-premise in Datacentre in Sydney, migrated to Azure Websites, along with all publishing, build and deployments.

## Prototyping
----------------------------

I prototyped the first iteration of the iAdviser web application in ASP.NET MVC around 2013 to 2015. This was full stack development backend/frontend and so on.
I approached it from a domain-driven design using best practices at the time.

Implemented repository-service patterns with SOLID principles where possible. LINQ to EF, Mapping.

With shared libraries I initially setup a hosted nuget server on ia's VM in the datacentre, and later migrated nuget projects to MyGet. Along with scripting processes to simplify nuget package creation.


## Database refactoring


One of the big challenges with implementing Entity Framework was subtle differences (extra fields, and tables) in the core databases across Australia and New Zealand, this was a database with ~800 tables in Sql Server... now with EF initially this meant having 2 different contexts to work across both, and using some funky magic to apply these different contexts across regions. This was a big time-sink.

So I recommended to consolidate Australian And New Zealand databases to match the same structure which would prove to be a big winner. The development manager did the actual database consolidation, and I updated all the web based projects.

- Built batch files to simplify EF7 reverse engineered database contexts, effectively to a one line call to completely refresh context, also allowed partial contexts (with specified tables)


## Report Re-Architecture

Over a period of approx 9 months and the implementation by a colleague, we re-architected how their report generation was done. Previously using Crystal Reports in the monolithic Desktop app, with a lot of SQL SPROCS to generate reports, done completely in the Winforms app. 

The new approach I recommended was to generate these complex reports entirely server side... html, csv, pdf, everything. Data would be sent up to the server as a json to specify criteria for report generation, it would return a generation "guid" and be processed via another service (Azure WebJob), the frontends could poll the api's for current generation status, details etc.

- Data querying was implemented using EF 5.0 Linq, Later migrated to 6.0.  The core database was ~800 tables in Sql Server.
- I created an interfaced component structure that was applied to query the data in a consistent approach and return models that could be used directly in the Razor views (with strong typing), effectively create a new class inheriting a base, override methods, and than these new reports were dynamically found without needing to register.
- Report templates generated using Razor-html-css, which significantly simplified report development once the developers were up to speed with razor.
- Reports consisted of multiple documents and were time-consuming. The solution to this was Azure Queues, Storage, WebJobs, along with MongoDb to store various stages of the generation.
- Reports generated using asynchronous tasks to handle concurrent processing, which also meant considering how the EF contexts were created and disposed for each one.
- The desktop app used this new report api, replacing crystal reports.

**Components**:

- Report API - Provided the web API's for the client apps to request a report. A request would be sent, than Azure Queues and MongoDb would have a record created.
- Report Generation - Actually generating the report data models from the database.
- HTML Template Generation - Composing HTML result pages with report data models using Razor
- HTML to PDF Conversion - this was a microservice api that used "Chromium" (the engine the powers Chrome) to render a PDF from HTML.
- Report Processer - This handled the full process in the background, concurrently.
- Frontend testing page
- Frontend client apps - Desktop

I created a frontend report testing page in native html,css and Javascript to test the end-to-end process of generation across dynamic sample reports. Polling was applied to update the u.i as it's processing.

I advised architectural design, did code reviews, refactoring, and assisted as needed for this process. 

There were several iterations in order to complete the process to suit all scenarios.

Now this was a project done before the introduction of .NET Core and using EF6.0 .Net Framework, and the biggest issue with migrating to .NET Core at this stage was updating the hundreds of EF LINQ queries to work properly with it.Instead I wrote some wrappers to use .NET Core's configuration so at least we could share the same configuration source as all the new .NET Core projects.



## Authentication Layers

With the new Web layers, there were multiple API's to serve different requests, so I created an Authentication microservice API to generate JSON Web Tokens that all the API's would consume.

These tokens would contain the build environment that the token is configured for, this build environment would than setup the Dependency Injections and configuration for the connections for that specific environment per request:

For example, "local", "dev", or "stage" stored in token, when request made than connection factories would be applied so when the D.I contexts and libraries get injected into the constructors, it would suit the correct environment. Simpler than it sounds

- EF 7
- Mongo
- Azure


## Azure

I initially implemented the Azure storage libraries, for azure queues, blobs, file shares. Than created wrappers to abstract the blob file stores, and other azure services.

- Created console apps to migrate binary data from database to Azure blobs, with resume so it could be stopped and restarted
- Created .NET Core Config Source Providers to use Azure File Shares as a multiple build environment config loader.
This meant that the json config files for all environments (local,dev,stage,live...etc there were more) can be stored in a central location on azure files, instead of within each API or Console application. CI/CD was also setup for the config repositories to update the Azure file shares when config files were updated. Than it was a simple restart on the apps to refresh configs.

- Created batch scripts to handle single line calls to start/stop/restart - create and delete web slots in Azure Websites

## DevOps

Yeah, batch files, powershell scripts, MSBuild builds and deployments, later migrated to VSTS (Azure DevOps).

Here I created the VSTS (now Azure DevOps) company login, set up the backend and frontend builds and releases.

- .NET Framework
- .NET Core
- NodeJS frontend based apps using Aurelia JS framework
- Azure File Share Deployments for JSON based configurations (created custom .NET Core Azure Config loaders)
- Created build pipelines 
- Created release pipelines for all apis, using multiple Web slots in Azure Websites to match the deployment environments.
- Created Task groups to share release deployments across slots without duplicating individual tasks, using variables and release name to match web deployment slot name.
- Created Task groups to post slack information after web deployments.
- Build Pipelines
- Release Pipelines

## Frontend Components

### General

Wrote general javascript for iAdviser prototype.

- Admin areas
- Datagrids
- Ajax Requests

Much of this was native es5 javascript, which later with the new versions we moved to Aurelia (similar to Angular).

Assisted development and teaching in Javascript for other projects.

### Online Quoting Wizard

Wrote a SPA online quoting wizard in native ES5 Javascript which had 5 steps, each step loaded it's data dynamically to save resources.

### Report Testing Page

Test all reports via sample json queries, with polling for u.i updates as generation is occuring on the server.

### Question Collection Editor

I built a frontend question collection editor entirely in javascript ES5 designed to handle their complex question form data. This was about 10 tables in the database to define question structure, formats, conditionals, calculations and so on.

This also consisted of backend model projections to/from the database to a clean structured set of viewmodels for the frontend, and api's to handle querying, and updating of responses.

Initially I created the frontend in ES5 Javascript with prototyped functions, than later wrote migration scripts to help migrate it to ES6 based classes, arrow syntax etc with the help of the `lebab` transpiler.

- input mask formatting for dates, numbers, currencies, and specific regex style responses.
- es5 prototyped functions, later converted to ES6 classes
- conditional questions based on answer responses
- dropdowns
- calculated questions (ie sum of sets of question answers, dollar figures etc)
- datagrids with rows of data.
- free text areas
- validation for all types
- grouped sets of questions with conditional logic to hide/show according to answers

## Testing

On the legacy desktop application, there was no unit testing. So on new web apps implemented unit testing with Xunit, TDD where possible.

Also created a large variety of linqpad scripts to help with scratchpad experiments, and even some simple tooling


## Tooling 

- I setup postman company login, and created the postman collections, and requests to test API endpoints.
- I created the repository folder structure
- I created the slack company login, and setup automatic slack notifications task groups in Azure DevOps to post deployment completions.
