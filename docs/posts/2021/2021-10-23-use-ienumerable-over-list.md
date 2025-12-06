---
title: "Use IEnumerable<T> over List for DTO, Models properties with collections"
categories:
  - Blog
tags:
  - .NET
  - List<T>
  - IEnumerable<T>
---

Over the past year or so I have seen developers using `List<T>` for poco properties, public method signatures and results. 

Here I will attempt to explain why it's generally a better idea to use `IEnumerable<T>` in most DTO's, public method result and parameter signatures over `List<T>`

## 1. Coding by abstraction rather than concrete implementation
---


An `IEnumerable<T>` represents the interface contract to iterate a series of items for reading, now from there you have `ICollection<T>` which provides abstractions for modifying a collection, and at a higher level that inherits both interfaces have `List<T>` which is a concrete implementation to add and modify that collection of items.

It's generally considered a best practice to code by interface (abstraction) rather than concrete implementation in order to reduce coupling.

### Example 1:

Consider the following:

```cs
public void ProcessItems(List<int> integers){
    ....  
}
```

Means that this method will only accept a list of integers that allows sorting, adding, deleting of that collection directly on the list.

vs the decoupled version which provides portability of types.

```cs
public void ProcessItems(IEnumerable<int> integers){
    ....  
}

```

> What about `IList<T>` as thats an interface?  .... well yes, though it inherits `IEnumerable<T>`, `ICollection<T>` and other interfaces to provide modification of a collection.



### Example 2:

```cs
public class AModel
{
  public List<string> Names {get; set;}
  public List<string> Items {get; set;}
}
```

these props are tightly coupled to `List<T>` (or decendant classes) and the collection itself can be directly modified.

vs the **decoupled** properties

```cs
public class AModel
{
  public IEnumerable<string> Names {get; set;}
  public IEnumerable<string> Items {get; set;}
}

```

now these props are decoupled from the implementation at a much lower level, and become more portable.


## 2. Immutable collection of elements
---


While the objects within `IEnumerable<T>` may change, the actual collection of items wont, without changing the entire collection. it reduces errors and promotes writing clean, concise, intent-revealing code.


## 3. LINQ
---

LINQ is an incredibly powerful and flexible expression language within .NET that naturally operates with `IEnumerable<T>`, take for instance the `.Where()`, `.Select()` methods (among a host of others) that accept and  return `IEnumerable<T>`. 

These can also be lazily evaluated if you like, for instance a `.ToList()` would execute the final result.

