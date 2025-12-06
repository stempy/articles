---
title: "How to create a nodejs microservice REST api with typescript, jest - PART 3 PUPPETEER"
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

# Part #3 - (Optional) Add Puppeteer

If you want to automate some functions, or test browser specific components, Googles puppeteer will allow you to do this.

Add puppeteer

> NOTE: this will by default download chromium which can be rather large

```sh
npm install puppeteer @types/puppeteer
```

## Add Jest helpers for puppeteer

Add jest helpers for unit testing

```sh
npm install jest-puppeteer
```

In `src/jest.config.js` add the `jest-puppeteer` preset, if applied PART 1, you would now have the following jest config

```js
// jest.config.js
module.exports = {
  preset: 'jest-puppeteer',
  reporters: [ "default", "jest-junit" ]
}
```

In `src/jest-puppeteer.config.js` add

```js
module.exports = {
	launch: {
		dumpio: true,
		headless: false,
		args: ['--disable-infobars'],
	},
	browserContext: 'default'
};
```

In `src/jest-global-setup.ts` add

```ts
const { setup: setupPuppeteer } = require('jest-environment-puppeteer');
/**
 * Sets up the environment for running tests with Jest
 */
module.exports = async function globalSetup(globalConfig: any) {
	// do stuff which needs to be done before all tests are executed
	await setupPuppeteer(globalConfig);
};
```

now, add typescript support for jest and puppeteer

```sh
npm install @types/jest-environment-puppeteer
```

## Recap

NPM install

```sh
echo add puppeteer
echo ===========================================
npm install puppeteer @types/puppeteer

echo add jest-puppeteer
echo ===========================================
npm install jest-puppeteer
npm install @types/jest-environment-puppeteer
```