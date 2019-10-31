---
title: Introduction
sidebar_label: Introduction
hide_title: true
---

# Cortex.Net

_State management like [MobX](https://mobx.js.org/README.html) for .NET_

## NuGet installation

To install the main library, install the the [Cortex.Net NuGet package](https://nuget.org/packages/Cortex.Net/). The main library allows you to compose observable reactive state yourself.

```powershell
PM> Install-Package Cortex.Net
```
To make life easier Cortex.Net supports weaving to create transparent observable state.

Install the [Cortex.Net.Fody NuGet package](https://nuget.org/packages/Cortex.Net.Fody/) and update the [Fody NuGet package](https://nuget.org/packages/Fody/):

```powershell
PM> Install-Package Fody
PM> Install-Package Cortex.Net.Fody
```

The `Install-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<CortextWeaver/>` to [FodyWeavers.xml](https://github.com/Fody/Home/blob/master/pages/usage.md#add-fodyweaversxml)

```xml
<Weavers>
  <CortextWeaver/>
</Weavers>
```

## Introduction

Cortex.Net is a library that makes state management simple and scalable by transparently applying [functional reactive programming](https://en.wikipedia.org/wiki/Functional_reactive_programming) (TFRP). It is more or less a direct port of the excellent [MobX](https://mobx.js.org/README.html) library. As C# has Class-based inheritance versus the Prototype-based inheritance model of JavaScript, porting the library introduced some unique challenges. These are mostly solved by [Weaving](https://github.com/Fody/Fody) your library of state objects.

The philosophy behind Cortex.Net is very simple:

_Anything that can be derived from the application state, should be derived. Automatically._

which includes the UI, data serialization, server communication, etc.

[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) and Cortex.Net together are a powerful combination. Blazor renders the application state by providing mechanisms to translate it into a tree of renderable components. Cortex.Net provides the mechanism to store and update the application state that Blazor then uses.

<img alt="Cortex.Net unidirectional flow" src="https://github.com/mobxjs/mobx/raw/master/docs/assets/flow.png" align="center" />

Both Blazor and Cortex.Net provide optimal and unique solutions to common problems in application development. Blazor provides mechanisms to optimally render UI by using a virtual DOM that reduces the number of costly DOM mutations. Cortex.Net provides mechanisms to optimally synchronize application state with your Blazor components by using a reactive virtual dependency state graph that is only updated when strictly needed and is never stale.

## Core concepts

Cortex.Net has only a few core concepts. The following snippets can be tried online using 

### Observable state
