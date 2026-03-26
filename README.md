[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.indexeddb.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.indexeddb/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.indexeddb/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.indexeddb/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.indexeddb.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.indexeddb/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.utils.indexeddb)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.indexeddb/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.indexeddb/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.Utils.IndexedDb
### A Blazor utility library for managing IndexedDB

## Installation

```bash
dotnet add package Soenneker.Blazor.Utils.IndexedDb
```

## Setup

Register services in `Program.cs`:

```csharp
builder.Services.AddIndexedDbUtilAsScoped();
```

Inject the higher-level utility where you need it:

```csharp
@inject IIndexedDbUtil IndexedDb
```

## Usage

Initialize the package once before first use, ensure the store exists, then read/write values by `databaseName`, `storeName`, and `key`:

```csharp
await IndexedDb.Initialize();

const string databaseName = "app";
const string storeName = "settings";
const string key = "theme";

await IndexedDb.EnsureStore(databaseName, storeName);
await IndexedDb.Set(databaseName, storeName, key, "dark");

string? theme = await IndexedDb.Get(databaseName, storeName, key);
bool hasTheme = await IndexedDb.ContainsKey(databaseName, storeName, key);
int count = await IndexedDb.GetLength(databaseName, storeName);
```

You can also store typed objects; values are JSON-serialized by `IIndexedDbUtil`:

```csharp
public sealed record UserPreference(string Theme, bool SidebarCollapsed);

await IndexedDb.Set("app", "preferences", "user:1", new UserPreference("dark", true));

UserPreference? preference = await IndexedDb.Get<UserPreference>("app", "preferences", "user:1");
```
