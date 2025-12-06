---
title: "How to create a nodejs microservice REST api with typescript, jest - PART 1 SETUP"
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
---

Recently, for a client project I was tasked with creating a nodejs based microservice, I've have created many nodejs projects over the years, though like to keep up with current approaches, so here is a quickstarter guide to get a modern nodejs service.

We will be using `npm` for package managemen rather than `yarn`, while I do like yarn, npm comes with node and will fit most peoples scenarios.

Lets start.

Pre-Requisites:
- NodeJs is installed, latest or LTS version
- An editor such as Visual Studio Code
- Basic familiarity with a command line, be it linux bash, or windows command line

# Step #1 - Setup the basics

Create a folder/directory to host our nodejs project. 

Create npm project and install typescript and ambient nodejs types (typescript definitions for built in nodejs functions)

```sh
npm init -y
npm install typescript --save-dev
npm install @types/node --save-dev
```

Create `tsconfig.json` with the following command line. 

Linux

```sh
npx tsc --init --rootDir src --outDir build \
--esModuleInterop --resolveJsonModule --lib es6 \
--module commonjs --allowJs true --noImplicitAny true
```

Windows

```cmd
npx tsc --init --rootDir src --outDir build --esModuleInterop -resolveJsonModule --lib es6 --module commonjs --allowJs true --noImplicitAny true
```

This will create a `tsconfig.json` similar to 

```js
{
  "compilerOptions": {
    /* Basic Options */
    // "incremental": true,                   /* Enable incremental compilation */
    "target": "es5",                          /* Specify ECMAScript target version: 'ES3' (default), 'ES5', 'ES2015', 'ES2016', 'ES2017', 'ES2018', 'ES2019' or 'ESNEXT'. */
    "module": "commonjs",                     /* Specify module code generation: 'none', 'commonjs', 'amd', 'system', 'umd', 'es2015', or 'ESNext'. */
    "lib": ["es6"],                     /* Specify library files to be included in the compilation. */
    "allowJs": true,                          /* Allow javascript files to be compiled. */
    // "checkJs": true,                       /* Report errors in .js files. */
    // "jsx": "preserve",                     /* Specify JSX code generation: 'preserve', 'react-native', or 'react'. */
    // "declaration": true,                   /* Generates corresponding '.d.ts' file. */
    // "declarationMap": true,                /* Generates a sourcemap for each corresponding '.d.ts' file. */
    // "sourceMap": true,                     /* Generates corresponding '.map' file. */
    // "outFile": "./",                       /* Concatenate and emit output to single file. */
    "outDir": "build",                          /* Redirect output structure to the directory. */
    "rootDir": "src",                         /* Specify the root directory of input files. Use to control the output directory structure with --outDir. */
    // "composite": true,                     /* Enable project compilation */
    // "tsBuildInfoFile": "./",               /* Specify file to store incremental compilation information */
    // "removeComments": true,                /* Do not emit comments to output. */
    // "noEmit": true,                        /* Do not emit outputs. */
    // "importHelpers": true,                 /* Import emit helpers from 'tslib'. */
    // "downlevelIteration": true,            /* Provide full support for iterables in 'for-of', spread, and destructuring when targeting 'ES5' or 'ES3'. */
    // "isolatedModules": true,               /* Transpile each file as a separate module (similar to 'ts.transpileModule'). */

    /* Strict Type-Checking Options */
    "strict": true,                           /* Enable all strict type-checking options. */
    "noImplicitAny": true,                    /* Raise error on expressions and declarations with an implied 'any' type. */
    // "strictNullChecks": true,              /* Enable strict null checks. */
    // "strictFunctionTypes": true,           /* Enable strict checking of function types. */
    // "strictBindCallApply": true,           /* Enable strict 'bind', 'call', and 'apply' methods on functions. */
    // "strictPropertyInitialization": true,  /* Enable strict checking of property initialization in classes. */
    // "noImplicitThis": true,                /* Raise error on 'this' expressions with an implied 'any' type. */
    // "alwaysStrict": true,                  /* Parse in strict mode and emit "use strict" for each source file. */

    /* Additional Checks */
    // "noUnusedLocals": true,                /* Report errors on unused locals. */
    // "noUnusedParameters": true,            /* Report errors on unused parameters. */
    // "noImplicitReturns": true,             /* Report error when not all code paths in function return a value. */
    // "noFallthroughCasesInSwitch": true,    /* Report errors for fallthrough cases in switch statement. */

    /* Module Resolution Options */
    // "moduleResolution": "node",            /* Specify module resolution strategy: 'node' (Node.js) or 'classic' (TypeScript pre-1.6). */
    // "baseUrl": "./",                       /* Base directory to resolve non-absolute module names. */
    // "paths": {},                           /* A series of entries which re-map imports to lookup locations relative to the 'baseUrl'. */
    // "rootDirs": [],                        /* List of root folders whose combined content represents the structure of the project at runtime. */
    // "typeRoots": [],                       /* List of folders to include type definitions from. */
    // "types": [],                           /* Type declaration files to be included in compilation. */
    // "allowSyntheticDefaultImports": true,  /* Allow default imports from modules with no default export. This does not affect code emit, just typechecking. */
    "esModuleInterop": true,                  /* Enables emit interoperability between CommonJS and ES Modules via creation of namespace objects for all imports. Implies 'allowSyntheticDefaultImports'. */
    // "preserveSymlinks": true,              /* Do not resolve the real path of symlinks. */
    // "allowUmdGlobalAccess": true,          /* Allow accessing UMD globals from modules. */

    /* Source Map Options */
    // "sourceRoot": "",                      /* Specify the location where debugger should locate TypeScript files instead of source locations. */
    // "mapRoot": "",                         /* Specify the location where debugger should locate map files instead of generated locations. */
    // "inlineSourceMap": true,               /* Emit a single file with source maps instead of having a separate file. */
    // "inlineSources": true,                 /* Emit the source alongside the sourcemaps within a single file; requires '--inlineSourceMap' or '--sourceMap' to be set. */

    /* Experimental Options */
    // "experimentalDecorators": true,        /* Enables experimental support for ES7 decorators. */
    // "emitDecoratorMetadata": true,         /* Enables experimental support for emitting type metadata for decorators. */

    /* Advanced Options */
    "resolveJsonModule": true                 /* Include modules imported with '.json' extension */
  }
}
```

Adjust what you want, and clean out the rest.

Now, lets create a `src` folder to host our source

```sh
mkdir src
```

create a `src/index.ts`

```ts
console.log('hello world');
```

## Add nodemon for development

Now, we want to have cold-reloading so the application will be re-built when we edit files for development.

```sh
npm install --save-dev ts-node nodemon
```

Add `nodemon.json` config file

```js
{
  "watch": ["src"],
  "ext": ".ts,.js",
  "ignore": [],
  "exec": "ts-node ./src/index.ts"
}
```

Add a script to `package.json` file:

```json
"start": "nodemon"
```

run `npm run start` to build and run our app, and watch for file changes, change `src/index.ts` and save to see changes applied.

## Add production build config

Install rimraf in order to clean before build

```sh
npm install --save-dev rimraf
```

Add to `package.json` scripts

```json
"clean": "rimraf ./build",
"build": "npm run clean && tsc",
"start:prod" : "npm run build && node build/index.js"
```

> IMPORTANT: We dont want the build folder in source control, so ensure to add a  `build/` path within a `.gitignore` file in the same path or higher in repository.

Now to build a production build (for instance in a build pipeline)

```sh
npm run build
```

And to just build and run for production

```sh
npm run start
```

## Jest Unit Testing

For unit testing, we will use the popular unit testing framework Jest

Install Jest and typescript types

```sh
npm install --save-dev jest @types/jest
```

And in `package.json` scripts

```json
"test": "jest"
```

Now you can just 

```sh
npm run test
```

## Add ts-jest for typescript support

Now we want jest tests themselves to be in typescript (or js if preferred)

```sh
npm install --save-dev ts-jest
```

add to `src/jest.config.js`

```json
module.exports = {
  testMatch: ["**/?(*.)+(spec|test).[t]s"],
	testPathIgnorePatterns: ['/node_modules/', 'dist','/build/'], // 
	setupFilesAfterEnv: ['<rootDir>/jest-setup.ts'],
	transform: {
        "^.+\\.ts?$": "ts-jest",
    },
}
```

### Optional jest junit reporting

For continuos integration pipelines such as azure pipelines, we want to produce unit test results file, one of them is junit. It may differ according to your CI host, however for Azure Pipelines it uses Junit.

For junit:

install

```sh
npm install --save-dev jest-junit
```

Add to `src/jest.config.js`

```js
{
  "reporters": [ "default", "jest-junit" ]
}
```

For your Continuous Integration pipeline you can run

```sh
jest --ci --reporters=default --reporters=jest-junit
```

or even add in `package.json`

```js
"test:ci":"jest --ci --reporters=default --reporters=jest-junit"
```

> IMPORTANT: exclude `junit.xml` from source control in `.gitignore` file in same path or higher

## Install Overview

Just to recap, here is the previous steps applied in one.

Install all the packages

Windows

```sh
echo init npm project
echo ========================================
npm init -y
npm install typescript --save-dev
npm install @types/node --save-dev

echo create tsconfig.json
echo ========================================
npx tsc --init --rootDir src --outDir build --esModuleInterop -resolveJsonModule --lib es6 --module commonjs --allowJs true --noImplicitAny true

echo create src
echo ========================================
if not exist src md src
echo console.log("hello world"); >src\index.ts

echo install nodemon
echo ========================================
npm install --save-dev ts-node nodemon

echo install rimraff
echo ========================================
npm install --save-dev rimraf

echo install jest
echo ========================================
npm install --save-dev jest @types/jest

echo install junit
echo ========================================
npm install --save-dev jest-junit

echo install ts-jest
echo ========================================
npm install --save-dev ts-jest

```

With the following configuration files:

in `package.json` scripts section

```json
"scripts": {
    "build": "rimraf ./build && tsc",
    "start": "nodemon",
    "start:prod": "npm run build && node build/index.js",
    "test": "jest --config src/jest.config.js",
    "test:ci": "jest --config src/jest.config.js --ci --reporters=default --reporters=jest-junit"
  },
```

in `nodemon.json`

```json
{
    "watch": ["src"],
    "ext": ".ts,.js",
    "ignore": [],
    "exec": "ts-node ./src/index.ts"
}
```

in `src/jest.config.js`

```js
// jest.config.js
module.exports = {
	testMatch: ["**/?(*.)+(spec|test).[t]s"],
	testPathIgnorePatterns: ['/node_modules/', 'dist','/build/'], // 
	setupFilesAfterEnv: ['<rootDir>/jest-setup.ts'],
	transform: {
        "^.+\\.ts?$": "ts-jest",
  }
};
```

