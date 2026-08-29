[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.indexeddb.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.indexeddb/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.indexeddb/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.indexeddb/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.indexeddb.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.indexeddb/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.utils.indexeddb)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.indexeddb/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.indexeddb/actions/workflows/codeql.yml)

# Soenneker.Blazor.Utils.IndexedDb

A scoped Blazor utility for storing strings and JSON-serialized .NET values in browser IndexedDB object stores.

The API manages simple key/value stores with string keys. It does not expose indexes, cursors, compound keys, or multi-operation transactions.

## Installation

```bash
dotnet add package Soenneker.Blazor.Utils.IndexedDb
```

```csharp
using Soenneker.Blazor.Utils.IndexedDb.Registrars;

builder.Services.AddIndexedDbUtilAsScoped();
```

```razor
@using Soenneker.Blazor.Utils.IndexedDb.Abstract
@inject IIndexedDbUtil IndexedDb
```

IndexedDB is a browser API. Call the service after interactive rendering, not during server prerendering.

## Store and retrieve a value

`Set` creates the database and object store when necessary:

```csharp
const string database = "my-app";
const string store = "preferences";

await IndexedDb.Set(database, store, "theme", "dark");

string? theme = await IndexedDb.Get(database, store, "theme");
bool exists = await IndexedDb.ContainsKey(database, store, "theme");
```

`Get` returns `null` when the database, store, or key does not exist. A stored empty string is returned as an empty string. Database names, store names, and keys must be non-empty strings.

`Initialize()` is optional; it eagerly loads the JavaScript module but does not open a database. `EnsureStore()` is useful when an application wants to provision a store before its first write.

## Store typed values

Non-string values are serialized to JSON before storage and deserialized on read:

```csharp
public sealed record UserPreference(string Theme, bool SidebarCollapsed);

var preference = new UserPreference("dark", true);
await IndexedDb.Set(database, store, "user:42", preference);

UserPreference? loaded =
    await IndexedDb.Get<UserPreference>(database, store, "user:42");
```

Values passed to either `Set` overload cannot be null. The store contains the JSON string, not a native IndexedDB structured-clone object. Incompatible JSON or a changed .NET model can cause deserialization to fail. Version stored models and handle migration in application code when their shape evolves.

`Get<T>` returns the default value both when a key is missing and when stored JSON represents `null`. Use `ContainsKey` when that distinction matters.

## Inspect and modify a store

```csharp
IReadOnlyList<UserPreference> preferences =
    await IndexedDb.GetAll<UserPreference>(database, store);

IReadOnlyList<string> keys = await IndexedDb.GetKeys(database, store);
int count = await IndexedDb.GetLength(database, store);

await IndexedDb.Remove(database, store, "user:42");
await IndexedDb.Clear(database, store);
```

`GetAll<T>` returns values without their keys. For non-string types, blank values and values deserialized as `null` are omitted. Each method runs its own IndexedDB transaction, so a sequence of calls is not atomic and another tab can change data between them.

## Database and schema behavior

```csharp
await IndexedDb.EnsureStore(database, "outbox");
await IndexedDb.DeleteDatabase(database);
```

Adding a missing store upgrades the IndexedDB database version. Open connections created by this library close when another context requests a version change, but unrelated connections in another tab can block an upgrade or deletion until they close.

`DeleteDatabase` removes every store and value in the named database. Treat the name as a trusted application constant and require deliberate user intent before exposing that operation in a UI.

Cancellation stops waiting for the Blazor interop call; browsers do not provide a reliable way to abort every IndexedDB open, upgrade, or deletion request after it has started. Verify state before retrying a cancelled destructive operation.

## Storage and security

IndexedDB is origin-scoped browser storage, not durable server storage. Browsers can evict it, users can clear it, private sessions can discard it, and writes can fail because of quota or policy. Handle exceptions and keep authoritative data elsewhere.

Data is not encrypted by this library and is readable by JavaScript running on the same origin. Do not store passwords, bearer tokens, private keys, or other high-value secrets. Treat retrieved values as untrusted input and validate them before use.
