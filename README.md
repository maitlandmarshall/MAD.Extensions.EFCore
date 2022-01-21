## MAD.Extensions.EFCore: A library for .NET Core & .NET that provides extended functionality for Entity Framework Core

[![NuGet](https://img.shields.io/nuget/v/MAD.Extensions.EFCore.svg)](https://www.nuget.org/packages/MAD.Extensions.EFCore/)
[![NuGet](https://img.shields.io/nuget/dt/MAD.Extensions.EFCore)](https://www.nuget.org/packages/MAD.Extensions.EFCore/)

MAD.Extensions.EFCore is inspired by borisdj's [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions) library, and currently provides an extension to "Upsert" (Insert or Update) an object with a single method call using Entity Framework Core:

```csharp
dbContext.Upsert(entity);
dbContext.SaveChanges();
```

# Table of Contents

* [Supported Platforms](#supported-platforms)
* [Upserting Objects](#upserting-objects)    
    * [Shadow Properties](#shadow-properties)    

### Supported Platforms

MAD.Extensions.EFCore currently supports the following platforms:

* .NET Standard 2.1
* .NET Core 3.1
* .NET 5
* .NET 6

### Upserting Objects

### Shadow Properties
