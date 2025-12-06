---
title: "Essential Microservice Components"
categories:
  - Blog
tags:
  - microservice
  - SAAS
---

I have spent the last decade or so converting monoliths to microservice based architectures, with the microservices being primarily in .NET (.NET Framework and .NET Core 2-6) with some NodeJs Microservices (for things like puppeteer or javascript specific services) and some python, and compiled a list of items that seem to form Microservice implementations at the microservice level.

Here are some key infrastructural components I have found that apply to microservices (which are essentially mini web backends) with some references to .NET specific tooling, though the concepts can apply to most platforms.

More to be added later.

- Apis
	- Api Gateways
		- proxy
	- Api Visibility
		- publicly accessible
		- private
	- Api routing and route naming, verbs
	- Api Style
		- RPC Remote Procedure Call style
		- REST style
	- Api Conventions and Standards
		- OpenAPI/Swagger
		- Serialization (Binary, JSON, MessagePack)
			- Json
			- MessagePack
	- Api Controllers
		- CRUD Verbs, POST, PUT, GET, DELETE, PATCH
		- Http Calls
			- POST to create often returns 201, 202 with location header set
			- GET often returns 200
			- DELETE to delete resource with id, often returning 200, 204
		- Handle errors
			- Middleware to handle unhandled exceptions to not duplicate exception handling
			- Return client validation errors with 400 with standard data structure providing key error details
			- Standardised [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807) to return 500 and 400 errors



- Microservice event pipeline
	- on-demand like call api get response once completed, ideal for simple CRUD
	- long-running, post call to api, with 202 accepted return immediately, poll a status uri until completed, get results
	- listen to message bus events, queues etc and process message, publish messages (or respond) for event notifications, status updates, etc.
	- apply job tracking to track new jobs coming in, processing, completed etc in a database.

- HealthChecks
	- azure blob storage
	- database connections
	- service process

- Authentication
	- Auth Server
	- Resource Servers
	- JWT tokens
	- Auth flows

- Authorization
	- Roles
	- Claims
	- Resource Specific Permissions
	- Authorization handlers

- Messaging
	- Message/Event bus
		- RabbitMQ
		- AzureServiceBus
		- EventGrid
		- Kafka
	- Message Abstractions
		- MassTransit
		- NServiceBus
	- Message Envelopes
	- Message protocols, conventions
		- Cloudevents.io


- DAL (Data Access Layers)
	- Sql Server
	- Postgressql
	- Mysql
	- MongoDb
	- Cosmos
	- Repositories
	- Entities
	- Mapping Entities with DTO's/Models
	- Migrations (an Example is Entity Framework)
		- Adding migrations
		- Updating databases
			- Sql scripts
			- EF Core Bundles

- Services
	- Business logic

- Configuration
	- Json
	- Azure Key Vault
	- Environment Variables
	- Command line
	- 3rd Party Config providers

- Dependency Injection
	- Singleton (Application Scoped)
	- Scoped (Per Request, or other Scope on demand)
	- Transient (per resolution)

- Logging and Telemetry
	- Logging providers
	- Logging Abstractions
	- Logging ingestors

- Localization
	- Language
	- Currency
	- Culture
	- Timezones

- Templates
	- server side to generate microservice components, base layers
	- client side to generate openapi clients, ui components

- Docker
	- docker files
	- docker compose
	- docker image builds
	- common docker services
		- azurite
		- sql server
		- rabbitmq
		- redis

- DevOps
	- Build Versioning
		- CI generated versions - Build numbering
		- gitversion
		- version endpoints
			- assembly version
			- informational version
	- Build Pipelines
		- build
		- unit tests
		- code coverage
		- code syntax formatting / linting
	- deployment pipelines
		- environment setups
			- app services / web servers / cloud
			- automated testing with tools like newman (Postman collection runner)
		- hosted
		- docker/kubernetes
		- per environment configurations
			- database connections
			- azure/aws storage connections
			- message bus connections
			- config options

- Hosting
	- Cloud
	- Scaling
		- scale up (vertically)
		- scale out (horizontally)
		- Autoscale conditions
	
