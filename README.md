## MAD.Extensions.EFCore: A library for .NET Core & .NET that provides extended functionality for Entity Framework Core

[![NuGet](https://img.shields.io/nuget/v/MAD.Extensions.EFCore.svg)](https://www.nuget.org/packages/MAD.Extensions.EFCore/)
[![NuGet](https://img.shields.io/nuget/dt/MAD.Extensions.EFCore)](https://www.nuget.org/packages/MAD.Extensions.EFCore/)

MAD.Extensions.EFCore is inspired by borisdj's [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions) library, and currently provides an extension to "Upsert" (Insert or Update) an object with a single method call using Entity Framework Core.

### Supported Platforms

MAD.Extensions.EFCore currently supports the following platforms:

* .NET Standard 2.1
* .NET Core 3.1
* .NET 5
* .NET 6

### Installation

* Nuget Package Manager: `MAD.Extensions.EFCore`
* Package Manager Console: `Install-Package MAD.Extensions.EFCore`
* .NET CLI: `dotnet add package MAD.Extensions.EFCore`

### Upserting Objects

To upsert an object using your DbContext, add the following using statement to your class to access the extension method:

```csharp
using MAD.Extensions.EFCore;
```

Then call the Upsert method and pass through your entity as a parameter:

```csharp
dbContext.Upsert(entity);
dbContext.SaveChanges();
```

### Shadow Properties

Previous versions of MAD.Extensions.EFCore required shadow property values to be set manually using the transformations action on the upsert method:

```csharp
dbContext.Upsert(entity, childEntity =>
{
    switch (childEntity)
    {
        case ChildEntity:
            dbContext.Entry(childEntity).Property("ParentId").CurrentValue = entity.Id;
            break;
    }
});

dbContext.SaveChanges();
```

As of v3.0.1 and v6.0.1, MAD.Extensions.EFCore will automatically populate these shadow properties for any child entities underneath the entity passed to the upsert method.