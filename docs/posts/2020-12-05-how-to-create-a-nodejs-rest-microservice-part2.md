---
title: "How to create a nodejs microservice REST api with typescript, jest - PART 2 EXPRESS AND SOCKET IO"
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

# Part #2 - Add Express

Let's add express for a web server, along with it's Typescript definitions

```sh
npm install express @types/express
```

And a helper to deal with cors

```sh
npm install cors
```

Now lets modify `src/index.ts` to provide our webserver 

```ts
import express from 'express';
import http from 'http';
const port = 3000; // port to listen
const cors = require('cors');

// Create a new express app instance
const app: express.Application = express();
app.use(cors())
let httpServer = require("http").Server(app);

app.get('/', function (req, res) {
    res.send("Hello world");    
});

const server = httpServer.listen(port, function () {
    console.log(`listening on http://localhost:${port}`);
});
```

and run 

```sh
npm run start
```

to start the webserver, and than open `http://localhost:3000` in your web browser to see the text "Hello world"

> STOP the web application by pressing Ctrl-C in the terminal or command prompt.

## (Optional) Add Socket.IO for easy Realtime communication

Add socket io to the project. 

> As of this writing, socket.io is using version `3.0.x`.... this means you need compatible v3 clients to work with it.

```sh
npm install socket.io @types/socket.io
```

Add `src/index.html` and include:

```html
<!doctype html>
<html>

<head>
    <title>Socket.IO chat</title>
    <style>
        * {margin: 0;padding: 0;box-sizing: border-box;}
        body {font: 13px Helvetica, Arial;}
        form {background: #000;padding: 3px;position: fixed;bottom: 0;width: 100%;}
        form input {border: 0;padding: 10px;width: 90%;margin-right: 0.5%;}
        form button {width: 9%;background: rgb(130, 224, 255);border: none;padding: 10px;        }
        #messages {list-style-type: none;margin: 0;padding: 0;}
        #messages li {padding: 5px 10px;}
        #messages li:nth-child(odd) {background: #eee;}
    </style>
</head>

<body>
    <ul id="messages"></ul>
    <form action="">
        <input id="m" autocomplete="off" /><button>Send</button>
    </form>
    <script src="/socket.io/socket.io.js"></script>
    <script>
        var socket = io();
    </script>
</body>

</html>
```

Now modify `src/index.ts` to include socket.io

```js
import express from 'express';
import http from 'http';
const port = 3000; // port to listen
const cors = require('cors');

// Create a new express app instance
const app: express.Application = express();
app.use(cors())
let httpServer = require("http").Server(app);

// set up socket.io and bind it to our http server.
let io = require("socket.io")(httpServer, {
    cors: {
        origin: '*'
    }
});

app.get('/', function (req, res) {
    res.sendFile(__dirname+'/index.html');   
});

const server = httpServer.listen(port, function () {
    console.log(`listening on http://localhost:${port}`);
});

// whenever a user connects on port via a websocket, log that a user has connected
io.on("connection", function (socket: any) {
    console.log("a user connected");
    
    socket.onAny((event:any, ...args: any) => {
        console.log(`Received: ${event}`);
        if (args && args.length)
        {
            args.forEach((arg: any) => {
                console.log(`\t${arg}`);
            });
        }
    });
    
    socket.on('disconnect',()=>{
        console.log(`user disconnected`);
    });
});
```

## (Optional) Add EJS (Embedded Javascript Templates)

> OPTIONAL: If you are not rendering an HTML based UI from the NODEJS app, you can skip this step.

Here we will add Embedded Javascript Templates (EJS), an HTML Template language for express. This is suited if you will be rendering views from this application, if the nodejs app is primarily a webapi without a UI, than skip this step.

```sh
npm install ejs
```

Now, make a new folder `/src/views` and create a file named `index.ejs`. Add the following to it.

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <title>My Node App</title>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css">
    <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">
</head>
<body>
    <div class="container">
        <h1 class="header">My Node App</h1>
        <a class="btn" href="/testme"><i class="material-icons right">arrow_forward</i>Test me!</a>
    </div>
</body>
</html>
```

Modify the `src/index.ts` file too

```ts
import express from 'express';
import http from 'http';
const port = 3000; // port to listen
const cors = require('cors');

// Create a new express app instance
const app: express.Application = express();
app.use(cors())

// add ejs templating
app.set("views", path.join(__dirname,"views"));
app.set("view engine","ejs");


let httpServer = require("http").Server(app);

app.get('/', function (req, res) {
    res.render("index");    
});

const server = httpServer.listen(port, function () {
    console.log(`listening on http://localhost:${port}`);
});
```

## Recap

install

```sh
echo install express
echo ========================================
npm install express @types/express

echo install socket.io
echo ========================================
npm install socket.io @types/socket.io

echo install ejs
echo ========================================
npm install ejs
echo if not exist src/views md src/views
```

