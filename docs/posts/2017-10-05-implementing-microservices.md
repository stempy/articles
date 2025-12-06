---
title: "Implementing Microservices"
categories:
  - Blog
tags:
  - link  
---

First, lets look at a Microservices definition from : https://microservices.io/

Microservices - also known as the microservice architecture - is an architectural style that structures an application as a collection of services that are

- Highly maintainable and testable
- Loosely coupled
- Independently deployable
- Organized around business capabilities
- Owned by a small team

Lets delve into this.

## Highly Maintainable and Testable
This generally means the codebase is clean, using best practices for design, has good code coverage with unit tests, is easy to debug and test in isolation, there shouldn't need to be other services involved in order to debug or test this single microservice.

In modern terms for example with REST API's we can use tools like Postman, fiddler, to test endpoints, and processes. Or potentially development queues.

This applies to any environment it's on.... for instance local development, dev, uat, staging, or production. With certain considerations when it goes into a production environment.

## Loosely coupled
This microservice is self-encapsulated and has low dependencies on other services. Similar in concept to how components use dependency injection and inversion of control to loosely couple.

## Independently deployable
This microservice can be deployed in isolation.

For instance in a CI/CD process, a microservice can be deployed without requiring other services to be deployed.

## Organized around business capabilities
Logical business units, the microservice fulfills one of these purposes, and one only. 

This is not quite the same as one single task or feature however, as one logical unit could comprise of several features within that unit.

For instance:

Logical Business units and features

- accounting
        - querying
        - updating
        - creation
- reporting
- image compression
- document management
- asset storage
- ordering
        - order submission
        - order querying
        - order updating
        - order management

# Owned by a small team
This microservice could essentially be it's own project if need be. Issues tracked, CI/CD processes etc.

