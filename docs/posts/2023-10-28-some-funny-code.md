---
title: "Some funny code moments"
categories:
  - Blog
tags:
  - Funny Code
  - Disaster Code
---

We have all done it at one point or another, written a piece of code and than just thought... "wtf" or "what was I thinking" or something like that. Those moments in lapses of judgement, low brain juice, lack of oxygen or whatever.

But sometimes there are ones which are well...... damn.... I have seen quite a few interesting pieces of the years. Though ones that stick out.

## Easy login

Many years ago when I was brought on to do some PHP maintenance for a high profile telecommunications accessories companies e-commerce website, I found a couple of gotcha's from the previous developers. One of them was in performing logins, now on this particular website it was using the equivalent of :

```mysql
SELECT * FROM users WHERE 'username'='[username-from-form]' and password='[password-from-form]'
```

> Notice the `[password-from-form]` . That was literally the value entered directly in the form, no sanitization, or validation was in place at the time.

Now this was around 2004 so things like SQL injection and the like were not so prevalent. Anyway all it would take to login as any user (including the admin user with admin priviledges) was to enter a nice single asterisk `*` in the login form to produce the following.

```mysql
SELECT * FROM users WHERE 'username'='[username-from-form]' and password='*'
```

Now '*' would mean match anything for that field!.

And voila now you could place nice orders, change orders, view and edit user details, and a variety of other cool things. 

I was very suprised to see a high profile company to have severe security issues such as this..... and yet there it was.

## Novel Database record update process.

> WARNING: Example of how NOT to perform updates

Working on a .NET Core / Blazor Server project and found a novel (by whoever created it) way to update records in a database using EF Core.

Now the client Blazor app was sending up a collection of items to update from a grid edit table. Not just a single row, rather the entire grid of rows. Odd, ok now the database update code went like this.

```
1. Delete all rows in the table. (yep all rows)
2. And add every single row back in from the collection sent from client, as the client had the entire list of rows from the table.
```

Voila!!!  Database records updated...... lol. 


