using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.IndexedDb.Abstract;

/// <summary>
/// Blazor interop for browser-facing functionality exposed by this package.
/// </summary>
public interface IIndexedDbInterop : IAsyncDisposable
{
    /// <summary>
    /// Ensures the JavaScript module for this package has been loaded and initialized.
    /// </summary>
    ValueTask Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the specified IndexedDB object store exists.
    /// </summary>
    ValueTask EnsureStore(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a stored string value by key, or null if the key does not exist.
    /// </summary>
    ValueTask<string?> Get(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all string values from the specified object store.
    /// </summary>
    ValueTask<IReadOnlyList<string>> GetAll(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value for the specified key.
    /// </summary>
    ValueTask Set(string databaseName, string storeName, string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stored value by key.
    /// </summary>
    ValueTask Remove(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all values from the specified object store.
    /// </summary>
    ValueTask Clear(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the specified key exists in the object store.
    /// </summary>
    ValueTask<bool> ContainsKey(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all keys in the specified object store.
    /// </summary>
    ValueTask<IReadOnlyList<string>> GetKeys(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of entries in the specified object store.
    /// </summary>
    ValueTask<int> GetLength(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified IndexedDB database.
    /// </summary>
    ValueTask DeleteDatabase(string databaseName, CancellationToken cancellationToken = default);
}
