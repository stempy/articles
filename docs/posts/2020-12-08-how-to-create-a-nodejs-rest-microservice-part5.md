---
title: "How to create a nodejs microservice REST api with typescript, jest - PART 5 Logging"
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
  - loose coupling
  - logging
---

## Part #5 - Add loosely coupled logging

### A brief lesson on Loose Coupling

A very important concept in software development is **loose coupling**. This allows us to code against *abstractions* without concrete dependencies in place. This is very important as it simplifies *unit testing*, along with being able to swap out *specific implementations* of components very simply, as we see fit. 

Logging is one of these cases where you really want to abstract from the beginning.

Why?

Well logging is something we tend to apply throughout all parts of a codebase, or *should* do. 

Consider if we added an `npm` package for a logging library, and just started using it's logging methods everywhere. This may be fantastic as we now have full logging in place. **However** what happens when an awesome new logging library comes out that implements super logging feature X, or Y, or buffers logs, or something that makes it worth changing to..... ok so we `npm` it and remove the old logging library from `package.json`...... and now find nothing compiles.

- as we have to change all the imports across every single file that uses the new logging library.
- the method names may be slightly different, for example, one library may use `log.logDebug`, and another may use `log.debug` to represent a debug log message... this means refactoring all instances of all methods across the entire codebase.

> Omg, this is terrible and will take forever, the biggger the codebase the more painful

And it is..... after refactoring *many large codebases* in various languages, I found this can be incredibly annoying..... the developers didn't take the time or concern to think about good software design, and just plugged in what would work for the short term.

Unless we **loosely couple** such an important component. We do this by creating an `Interface` in TypeScript that represents what a Logger should do but NOT how it's done. And than we import this interface everywhere we need logging. We than `inject` this interface into classes and store a class property variable assigned to it. We than use the interfaced loggers methods everywhere for logging.

Ok, but won't actually log anything.

Now this is where we create the *implementation* of that interface. We write up the logging methods that implement the logging interface, and use the explicit logging library we want to apply, or our own. Well we could create multiple implementations, and use multiple logging libraries... the main point is now implementation is separated from the interface.

We than `inject` this logging library into the classes where we have defined the loggers interface. If we ever want to change implementation, we simply change what get's injected, usually in what's called a dependency injection container. however it's one or two places, rather than the multitude of classes.... this makes it very simple to swap out different libraries.

> Native Javascript does not have the concept of interfaces. It's one of the features of typing with TypeScript.

### Creating a simple logger with loose coupling

First, we want to define an interface that defines our abstraction

`src/ILogger.ts`

```ts
export interface ILogger {
    debug(msg:string, ...data: any[]):void;
    warn(msg:string, ...data: any[]):void;
    error(msg:string, ...data: any[]):void;
    info(msg:string, ...data: any[]):void;
}
```

Now, let's wire up a class that implements a simple console based logger of the `ILogger` interface

`src/loggers/ConsoleLogger.ts`

```ts
import { ILogger } from "./ILogger";

export class ConsoleLogger implements ILogger {

    debug(msg: string, ...data: any[]): void {
        this.emitLogMessage("debug",msg,data);
    }
    warn(msg: string, ...data: any[]): void {
        this.emitLogMessage("warn",msg,data);
    }
    error(msg: string, ...data: any[]): void {
        this.emitLogMessage("error",msg,data);
    }
    info(msg: string, ...data: any[]): void {
        this.emitLogMessage("info",msg,data);
    }

    private emitLogMessage(msgType: "debug"| "info" | "warn" | "error", msg:string, data:any[]){
        const msgFormat = `[${msgType}]`;
        const msgPrefix = `${new Date().toISOString().replace(/T/, ' ').replace(/\..+/, '')} ${msgFormat.padEnd(7)}`;
        
        if (data.length>0){
            console[msgType](msgPrefix, msg, data)
        } else {
            console[msgType](msgPrefix, msg);
        }
    }
}
```

Now, lets add the `ILogger` interface to a class say `src/TestLogging.ts`

> NOTE: typically we would test using unit tests, however for a simple example here it is.

```ts
import { ILogger } from './ILogger';

export class TestLogging
{
    private log: ILogger;
    
    constructor(logger:ILogger){
      this.log = logger;
    }

    public testLogging() {
        this.log.debug('test debug logging');
        this.log.info('test info logging');
        this.log.warn('a warning has occurred');
        this.log.error('an error has occured');

        // test a structured log type message
        this.log.info('test log with data', {"id":99, "text":"sometext"});
        this.log.info('another format for structured logging {0}',"sometext")
    }
}
```

Ok, now we could create a very simple injection layer (note there are various ways of applying dependency injection)

say `src/index.ts`

```ts
import { ConsoleLogger } from './loggers/ConsoleLogger';
import { TestLogging } from './TestLogger';

// create instance of consoleLogger
const consoleLogger = new ConsoleLogger();

// here we inject the consoleLogger which implements ILogger
const testLogger = new TestLogging(consoleLogger);
testLogger.testLogging();
```

Now if you ever want to change the logging from ConsoleLogger to a more advanced logger, you can do that in one place, instead of all the classes that use it's interface, they will remain the same.

### Now lets add Winston

Ok, so you now have a nice simple logger that can log to the console, but very limited, you might want to log to a file, to a thirdparty logging service or so on.

Well, lets do that now....... we will implement **winston**, a very popular nodejs logging library.

Install winston

```sh
npm install winston
```

And 

create `src/loggers/WinstonLogger.ts` with the following:

```ts
import { ILogger } from "../ILogger";

const winston = require('winston');
const logger = winston.createLogger({
    transports: [
        new winston.transports.Console()
    ]
});

/**
 * A Single ILogger implementation using the popular winston logging library
 * npm i winston
 */
export class WinstonLogger implements ILogger
{
    debug(msg: string, ...data: any[]): void {
        logger.debug(msg,data);
    }
    warn(msg: string, ...data: any[]): void {
        logger.warn(msg,data);
    }
    error(msg: string, ...data: any[]): void {
        logger.error(msg,data);
    }
    info(msg: string, ...data: any[]): void {
        logger.info(msg,data);
    }
}
```

Now, modify the `src/index.ts` file to use the winston logger:

```ts
import { WinstonLogger } from './loggers/WinstonLogger';
import { TestLogging } from './TestLogger';

// create instance of consoleLogger
const logger = new WinstonLogger();

// here we inject the consoleLogger which implements ILogger
const testLogger = new TestLogging(logger);
testLogger.testLogging();
```

And now you have logging processed via winston, by adding a file, and changing the index file, you don't have to go through each class that uses logging.

Simple.

You can view the source for this article at [nodejs-logging-abstractions](https://github.com/stempy/nodejs-logging-abstractions) in github
