---
title: Introduction
sidebar_label: Introduction
hide_title: true
---

# Cortex.Net

State management like MobX for .NET

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
