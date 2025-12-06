---
title: "How to create a nodejs microservice REST api with typescript, jest - PART 4 JSDOM and MONGODB"
categories:
  - Blog
tags:
  - link
  - typescript
  - nodejs
  - npm
  - nodemon
  - jest
  - microservice
  - rest api
  - mongodb
  - jsdom
  - mongoose
---

# Part #4 - (Optional) Add JSDOM

JSDOM is a great tool for working with a virtual browser DOM, for instance parsing HTML/XML, and manipulations, in browser standard way with javascript.

Install jsdom and typescript definitions

```sh
npm install jsdom @types/jsdom
```

# Part #5 - (Optiona) Add Mongodb as a Datastore (excluding mongoose)

Here we will want to add the official nodejs mongodb driver from [https://docs.mongodb.com/drivers/node/quick-start](https://docs.mongodb.com/drivers/node/quick-start){:target="_blank"}

Now there is a very popular npm library called mongoose which provides additional features such as defining schemas, db validations etc. I don't personally recommend it for the following reasons:

- Defining a schema is somewhat irrelevant when using TypeScript as we can type our models
- Validation provided by mongoose is all handled at the application layer, within the nodejs app, however modern web applications often use multiple stacks, and if the database is read/updated outside the application, all those schema definitions and validations by mongoose are not used. This is the issue with application level schema validation.
- MongoDb provides db level validation, which will apply regardless of how it's used.
- MongoDb tools such as the mongo cli & RoboMongo will use the same javascript apis to query and manipulate the database as the native driver.. if you use mongoose, you will have to translate the mongoose calls to the native mongodb queries for analysis with these tools.
- You can read more about this at [https://developer.mongodb.com/article/mongoose-versus-nodejs-driver](https://developer.mongodb.com/article/mongoose-versus-nodejs-driver){:target="_blank"}

```sh
npm install mongodb @types/mongodb
```

