## MAD.Extensions.EFCore: A library for .NET Core & .NET that provides extended functionality for Entity Framework Core

MAD.Extensions.EFCore is inspired by borisdj's [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions) library, and currently provides an extension to "Upsert" (Insert or Update) an object using the Entity Framework Core DbContext:

```csharp
dbContext.Upsert(entity);
```

# Table of Contents

* [Supported Platforms](#supported-platforms)
* [Installation](#installation)

### Supported Platforms

MAD.Extensions.EFCore currently supports the following platforms and any .NET Standard 2.1 target:

* .NET Core 3.1
* .NET 5
* .NET 6

### Installation

MAD.Extensions.EFCore can be installed using the following methods:

Visual Studio Nuget Package Manager:

`MAD.Extensions.EFCore`

Visual Studio Package Manager Console:

`PM> Install-Package MAD.Extensions.EFCore`

.NET CLI:

`> dotnet add package MAD.Extensions.EFCore`