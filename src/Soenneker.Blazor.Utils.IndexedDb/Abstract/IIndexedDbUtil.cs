using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.IndexedDb.Abstract;

/// <summary>
/// A higher-level Blazor utility built on top of <see cref="IIndexedDbInterop"/>.
/// </summary>
public interface IIndexedDbUtil
{
    /// <summary>
    /// Ensures the underlying JavaScript module has been loaded and is ready for use.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the Indexed Db is ready for use.</returns>
    ValueTask Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the specified IndexedDB object store exists.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the ensure store operation is complete.</returns>
    ValueTask EnsureStore(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a stored string value by key, or null if the key does not exist.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the text returned by get.</returns>
    ValueTask<string?> Get(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a JSON-serialized value by key, or default if the key does not exist.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the indexed db.</typeparam>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the value returned by get.</returns>
    ValueTask<T?> Get<T>(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all JSON-serialized values from the specified object store.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the indexed db.</typeparam>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by get All.</returns>
    ValueTask<IReadOnlyList<T>> GetAll<T>(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value for the specified key.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="value">Value to serialize and store under the specified key.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the set operation is complete.</returns>
    ValueTask Set(string databaseName, string storeName, string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a JSON-serialized value for the specified key.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the indexed db.</typeparam>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="value">Value to serialize and store under the specified key.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the set operation is complete.</returns>
    ValueTask Set<T>(string databaseName, string storeName, string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stored value by key.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the remove operation is complete.</returns>
    ValueTask Remove(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all values from the specified object store.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the Indexed Db has been cleared.</returns>
    ValueTask Clear(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the specified key exists in the object store.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>true if the specified key exists in the target store; otherwise, false.</returns>
    ValueTask<bool> ContainsKey(string databaseName, string storeName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all keys in the specified object store.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by get Keys.</returns>
    ValueTask<IReadOnlyList<string>> GetKeys(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of entries in the specified object store.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="storeName">Name of the target object store.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested value.</returns>
    ValueTask<int> GetLength(string databaseName, string storeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified IndexedDB database.
    /// </summary>
    /// <param name="databaseName">Name of the target database.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes after the targeted files have been deleted.</returns>
    ValueTask DeleteDatabase(string databaseName, CancellationToken cancellationToken = default);
}
